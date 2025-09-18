using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using NxB.Domain.Common.Constants;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Model;
using NxB.Dto.MemCacheActor;
using NxB.Dto.TallyWebIntegrationApi;
// TODO: Remove Service Fabric dependency when migration is complete
// using NxB.MemCacheActor.Interfaces;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Controllers.TallyWebIntegration
{
    [Produces("application/json")]
    [Route("crosscheck")]
    [Authorize(Roles = "Admin")]
    [ApiValidationFilter]
    public class CrossCheckController : BaseController
    {
        private static string CROSSCHECK_LASTID = "CrosscheckLastId";
        private static Guid FAKE_CROSSCHECK_TENANT_ID = Guid.Parse("00000000-0000-0000-0000-000000000001");

        private readonly AppTallyDbContext _appTallyDbContext;
        private readonly AppDbContext _appDbContext;
        private readonly TelemetryClient _telemetryClient;
        private readonly ITConService _tconService;
        private readonly ICounterIdProvider _counterIdProvider;

        public CrossCheckController(AppTallyDbContext appTallyDbContext, AppDbContext appDbContext, TelemetryClient telemetryClient, ITConService tconService, ICounterIdProvider counterIdProvider)
        {
            _appTallyDbContext = appTallyDbContext;
            _appDbContext = appDbContext;
            _telemetryClient = telemetryClient;
            _tconService = tconService;
            _counterIdProvider = counterIdProvider;
        }

        [HttpPost]
        [Route("addlatestoutcodestoingate")]
        public async Task<int> AddLatestOutGateCodesToInGate([FromBody] List<int> excludeCodes)
        {
            int codesAddedToInGateRadio = 0;

            try
            {
                var lastIndex = _counterIdProvider.Get(CROSSCHECK_LASTID, 0, FAKE_CROSSCHECK_TENANT_ID);
                var newLastIndex = await _appTallyDbContext.TConTBDAccessLogs.MaxAsync(x => x.Idx);

                var gateOutRadios = await GetOutGates();
                var gateRadioOutIds = gateOutRadios.Select(x => x.RadioAddress).ToList();
                var accessLogs = await _appTallyDbContext.TConTBDAccessLogs.Where(x => x.Idx > lastIndex)
                    .OrderByDescending(x => x.Idx).Where(x => !x.Rejected && gateRadioOutIds.Contains(x.RadioAddr) && !excludeCodes.Contains(x.Code))
                    .ToListAsync();
                var accessLogGroups =
                    accessLogs.GroupBy(x => x.RadioAddr, (radioAddress, logs) => new { radioAddress, logs });

                _telemetryClient.TrackTrace($"crosscheck: lastIndex={lastIndex}, newLastIndex={newLastIndex} found no of logs: {accessLogs.Count()}");
                foreach (var accessLogGroup in accessLogGroups)
                {
                    try
                    {
                        var gateOutRadio = gateOutRadios.SingleOrDefault(x => x.RadioAddress == accessLogGroup.radioAddress);
                        var tconService =
                            _tconService.CloneWithCustomClaimsProvider(
                                TemporaryClaimsProvider.CreateAdministrator(gateOutRadio.TenantId));

                        var gateInRadios = await GetInGates();
                        var matchingGateInRadios = gateInRadios.Where(x => x.TenantId == gateOutRadio.TenantId).ToList();

                        if (matchingGateInRadios.Count == 0)
                        {
                            _telemetryClient.TrackTrace($"crosscheck: No matching in gate for out-gate: {gateOutRadio.RadioAddress}");
                        }
                        foreach (var gateInRadio in matchingGateInRadios)
                        {
                            foreach (var accessLog in accessLogGroup.logs)
                            {
                                try
                                {
                                    var radioCode = await _tconService.FindRadioAccessCodeOrDefault(gateOutRadio.RadioAddress, accessLog.Code);
                                    await tconService.AddRadioAccessCodeIfNotAlreadyAdded(accessLog.Code, accessLog.KeyCode, radioCode?.Option ?? 0,
                                        gateInRadio.RadioAddress);
                                    _telemetryClient.TrackTrace($"crosscheck: adding code: {accessLog.Code} to gate-in: {gateInRadio.RadioAddress}");
                                    codesAddedToInGateRadio++;
                                    await _appTallyDbContext.SaveChangesAsync();
                                }
                                catch (Exception exception)
                                {
                                    _telemetryClient.TrackTrace($"Error doing crosscheck for GateIn: {gateInRadio.RadioAddress}, Code: {accessLog.Code}" + exception.Message, SeverityLevel.Error);
                                    _telemetryClient.TrackException(exception);
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        _telemetryClient.TrackTrace($"Error doing crosscheck for GateOut: {accessLogGroup.radioAddress}" + exception.Message, SeverityLevel.Error);
                        _telemetryClient.TrackException(exception);
                    }
                }

                if (newLastIndex != lastIndex)
                {
                    _counterIdProvider.Set(CROSSCHECK_LASTID, newLastIndex, FAKE_CROSSCHECK_TENANT_ID);
                    await _appDbContext.SaveChangesAsync();
                }
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackTrace("Error doing crosscheck: " + exception.Message, SeverityLevel.Error);
                _telemetryClient.TrackException(exception);
                throw;
            }

            if (codesAddedToInGateRadio > 0)
            {
                var logStr = $"CrossCheckService.AddLatestOutGateCodesToInGate added total of {codesAddedToInGateRadio} code to GateIn accesses";
                //_telemetryClient.TrackEvent(logStr);
                //Debug.WriteLine(logStr);
            }

            return codesAddedToInGateRadio;
        }

        private async Task<List<TallyGateDto>> GetOutGates()
        {
            // TODO: Implement caching without Service Fabric
            // var proxy = ServiceProxyHelper.CreateActorServiceProxy<IMemCacheActor>(0);
            // var _gateOutRadios = await proxy.GetCachedGates(MemCacheActorConstants.CACHED_GATES_OUT);
            // if (_gateOutRadios != null)
            // {
            //     return _gateOutRadios;
            // }

            _gateOutRadios = new List<TallyGateDto>();
            var gateOutRadios = await _appTallyDbContext.TConTWIStatuses.Where(x => x.Modus == (byte)TWIModus.GateOut)
                .Join(_appTallyDbContext.TConRadios, twiStatus => twiStatus.RadioAddr, radio => radio.RadioAddr,
                    (twiStatus, radio) => new { twiStatus, radio }).ToListAsync();
            foreach (var gateOutRadio in gateOutRadios)
            {
                var map = await _appDbContext.TallyMasterRadioTenantMaps.SingleOrDefaultAsync(x =>
                    x.TallyMasterRadioId == gateOutRadio.radio.MasterAddr);
                var radioAddress = gateOutRadio.radio.RadioAddr;

                if (map == null)
                {
                    _telemetryClient.TrackTrace(
                        $"CrossCheckService.LoadOutGates could not map radio with id {radioAddress} to tenant",
                        SeverityLevel.Error);
                    continue;
                }

                _gateOutRadios.Add(new TallyGateDto { TenantId = map.TenantId, RadioAddress = radioAddress });
            }

            if (Debugger.IsAttached)
            {
                // _gateOutRadios.Add(new TallyGateDto { TenantId = AppConstants.DEMONSTRATION_TENANT_ID, RadioAddress = 103042 });
            }
            _telemetryClient.TrackTrace($"CrossCheckService.Loaded {_gateOutRadios.Count} outgates: " + string.Join(',', _gateOutRadios.Select(x => x.RadioAddress)));
            // TODO: Implement caching without Service Fabric
            // await proxy.SetCachedGates(MemCacheActorConstants.CACHED_GATES_OUT, _gateOutRadios);
            return _gateOutRadios;
        }

        private async Task<List<TallyGateDto>> GetInGates()
        {
            // TODO: Implement caching without Service Fabric
            // var proxy = ServiceProxyHelper.CreateActorServiceProxy<IMemCacheActor>(0);
            // var _gateInRadios = await proxy.GetCachedGates(MemCacheActorConstants.CACHED_GATES_IN);
            // if (_gateInRadios != null)
            // {
            //     return _gateInRadios;
            // }

            _gateInRadios = new List<TallyGateDto>();
            var gateInRadios = await _appTallyDbContext.TConTWIStatuses.Where(x => x.Modus == (byte)TWIModus.GateIn).Join(_appTallyDbContext.TConRadios, twiStatus => twiStatus.RadioAddr, radio => radio.RadioAddr, (twiStatus, radio) => new { twiStatus, radio }).ToListAsync();
            foreach (var gateInRadio in gateInRadios)
            {
                var map = await _appDbContext.TallyMasterRadioTenantMaps.SingleOrDefaultAsync(x => x.TallyMasterRadioId == gateInRadio.radio.MasterAddr);
                var radioAddress = gateInRadio.radio.RadioAddr;

                if (map == null)
                {
                    _telemetryClient.TrackTrace($"CrossCheckService.LoadInGates could not map radio with id {radioAddress} to tenant", SeverityLevel.Error);
                    continue;
                }
                _gateInRadios.Add(new TallyGateDto { TenantId = map.TenantId, RadioAddress = radioAddress });
            }

            if (Debugger.IsAttached)
            {
                _gateInRadios.Add(new TallyGateDto { TenantId = AppConstants.DEMONSTRATION_TENANT_ID, RadioAddress = 100507 });
            }

            // TODO: Implement caching without Service Fabric
            // await proxy.SetCachedGates(MemCacheActorConstants.CACHED_GATES_IN, _gateInRadios);
            _telemetryClient.TrackTrace($"CrossCheckService.Loaded {_gateInRadios.Count} ingates: " + string.Join(',', _gateInRadios.Select(x => x.RadioAddress)));
            return _gateInRadios;
        }
    }
}
