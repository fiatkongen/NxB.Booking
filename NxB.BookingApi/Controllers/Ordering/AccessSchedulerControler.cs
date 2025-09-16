using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using NxB.Dto.Clients;
using NxB.Dto.OrderingApi;
using NxB.Dto.TallyWebIntegrationApi;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;
using NxB.Remoting.Interfaces.TallyWebIntegrationApi;

namespace NxB.BookingApi.Controllers.Ordering
{
    [Produces("application/json")]
    [Route("accessschedule")]
    [Authorize(Roles = "Admin")]
    [ApiValidationFilter]
    public class AccessSchedulerController : BaseController
    {
        private readonly AppDbContext _appDbContext;
        private readonly IRadioAccessCodeClient _radioAccessCodeClient;
        private readonly TelemetryClient _telemetryClient;
        private readonly IMapper _mapper;

        public AccessSchedulerController(AppDbContext appDbContext, IRadioAccessCodeClient radioAccessCodeClient, TelemetryClient telemetryClient, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _radioAccessCodeClient = radioAccessCodeClient;
            _telemetryClient = telemetryClient;
            _mapper = mapper;
        }

        [HttpPost]
        [Route("oneoffaccess/deactivate")]
        public async Task<List<int>> DeactivateOneOffAccesses()
        {
            var yesterDay = DateTime.Now.AddDays(-1).ToEuTimeZone();
            List<int> deactivatedAccessCodes = new List<int>();

            var now = DateTime.Now.ToEuTimeZone();
            var toOldAccesses = (await _appDbContext.Accesses.Where(x => (x.AccessType == AccessType.OneOff && x.DeactivationDate == null && x.ActivationDate < yesterDay) || (x.AutoDeactivationDate.HasValue && x.AutoDeactivationDate.Value < now && x.DeactivationDate == null)).ToListAsync()).ToList();
            if (toOldAccesses.Count > 0)
            {
                var newlyDeactivated = await DeactivateAccesses(toOldAccesses, (access) =>
                {
                    var radioAccessCodeTenantDto = new RadioAccessCodeTenantDto(access.Id, access.IsKeyCode, (uint)access.Code, access.TenantId);
                    return _radioAccessCodeClient.DeleteRadioAccessesCodeFromRadioCodes(new List<RadioAccessCodeTenantDto> { radioAccessCodeTenantDto });
                });
                await _appDbContext.SaveChangesAsync();
                deactivatedAccessCodes = deactivatedAccessCodes.Concat(newlyDeactivated).ToList();
            }

            var oneOffAccesses = (await _appDbContext.Accesses.Where(x => x.AccessType == AccessType.OneOff && x.DeactivationDate == null).ToListAsync()).ToList();
            if (oneOffAccesses.Count > 0)
            {
                var newlyDeactivated = await DeactivateAccesses(oneOffAccesses, (access) =>
                {
                    var radioAccessCodeTenantDto = new RadioAccessCodeTenantDto(access.Id, access.IsKeyCode, (uint)access.Code, access.TenantId, access.ActivationDate);
                    return _radioAccessCodeClient.DeleteRadioAccessesCodeFromRadioCodesIfActivatedInLog(new List<RadioAccessCodeTenantDto> { radioAccessCodeTenantDto });
                });
                await _appDbContext.SaveChangesAsync();
                deactivatedAccessCodes = deactivatedAccessCodes.Concat(newlyDeactivated).ToList();
            }

            if (deactivatedAccessCodes.Count > 0)
            {
                var logStr = $"SchedulerController.DeactivateOneOffAccesses deactivated total of {deactivatedAccessCodes.Count} oneoff accesses";
                //_telemetryClient.TrackEvent(logStr); //too much logging
                Debug.WriteLine(logStr);
            }

            return deactivatedAccessCodes;
        }

        private async Task<List<int>> DeactivateAccesses(List<Access> accesses, Func<Access, Task<List<int>>> deactivateAccessFunc)
        {
            List<int> deletedRadioCodes = new List<int>();

            foreach (var access in accesses)
            {
                try
                {
                    var deletedCodes = await deactivateAccessFunc(access);

                    if (access.AccessType == AccessType.OneOff)
                    {
                        if (deletedCodes.Count > 0)
                        {
                            access.Deactivate();
                        }
                        else if (access.AccessType == AccessType.Default)
                        {
                            access.Deactivate();
                        }

                        deletedRadioCodes = deletedRadioCodes.Concat(deletedCodes).ToList();
                    }

                    if (deletedCodes.Count > 0)
                    {
                        var logStr = $"SchedulerController.DeactivateOneOffAccesses deactivated code:  {access.Code} from {deletedCodes.Count} radios";
                        //_telemetryClient.TrackEvent(logStr); //too much logging
                        Debug.WriteLine(logStr);
                    }
                }
                catch (Exception exception)
                {
                    var logStr = $"SchedulerController.DeactivateOneOffAccesses error deactivating code: " + access.Code;
                    _telemetryClient.TrackEvent(logStr);
                    _telemetryClient.TrackException(exception);
                    Debug.WriteLine(logStr);
                }
            }

            return deletedRadioCodes;
        }

        [HttpPost]
        [Route("accessesarrived/activate")]
        public async Task<int> ActivateScheduledArrivedAccesses()
        {
            var now = DateTime.Now.AddMinutes(5).ToEuTimeZone();

            if (Debugger.IsAttached)
            {
                //now = now.AddHours(1);
            }

            var accesses = (await _appDbContext.Accesses.Where(x => x.AccessType == AccessType.Default && x.ActivationDate == null && x.AutoActivationDate <= now && x.DeactivationDate == null).ToListAsync()).ToList();
            if (accesses.Count > 0)
            {
                await ActivateAccesses(accesses);
                var logStr = $"SchedulerController.ActivateArrivedAccesses autoactivated {accesses.Count} accesses";
                _telemetryClient.TrackEvent(logStr);
                Debug.WriteLine(logStr);
            }

            return accesses.Count;
        }

        private async Task ActivateAccesses(List<Access> accesses)
        {
            foreach (var access in accesses)
            {
                try
                {
                    var radioAccessCodeTenantDto = _mapper.Map<RadioAccessCodeTenantDto>(access);
                    await _radioAccessCodeClient.AddRadioAccessesCodeToRadioCodes(new List<RadioAccessCodeTenantDto> { radioAccessCodeTenantDto });
                    access.Activate();
                    await _appDbContext.SaveChangesAsync();

                    var logStr = $"SchedulerController.ActivateAccesses autoactivated code: " + access.Code;
                    _telemetryClient.TrackEvent(logStr);
                    Debug.WriteLine(logStr);
                }
                catch (Exception exception)
                {
                    var logStr = $"SchedulerController.ActivateAccesses error autoactivating code: " + access.Code;
                    _telemetryClient.TrackEvent(logStr);
                    _telemetryClient.TrackException(exception);
                    Debug.WriteLine(logStr);
                }
            }
        }
    }
}
