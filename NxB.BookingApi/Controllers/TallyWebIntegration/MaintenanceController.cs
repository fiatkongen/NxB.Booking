using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.MemCacheActor;
// TODO: Remove Service Fabric dependency when migration is complete
// using NxB.MemCacheActor.Interfaces;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Controllers.TallyWebIntegration
{
    [Produces("application/json")]
    [Route("maintenance")]
    [Authorize(Roles = "Admin")]
    [ApiValidationFilter]
    public class MaintenanceController : BaseController
    {
        private readonly AppTallyDbContext _appTallyDbContext;
        private readonly IClaimsProvider _claimsProvider;
        private readonly ITConService _tConService;
        private readonly SystemLogQuery _systemLogQuery;
        private readonly AliveMonitorQuery _aliveMonitorQuery;
        private readonly ITallyMonitor _tallyMonitor;
        private readonly ITConMasterRadioTenantMapRepository _tconMasterRadioTenantMapRepository;
        private readonly IMasterRadioRepository _masterRadioRepository;
        private readonly TelemetryClient _telemetryClient;

        public MaintenanceController(IClaimsProvider claimsProvider, ITConService tConService, AppTallyDbContext appTallyDbContext, SystemLogQuery systemLogQuery, ITConMasterRadioTenantMapRepository tconMasterRadioTenantMapRepository, AliveMonitorQuery aliveMonitorQuery, ITallyMonitor tallyMonitor, IMasterRadioRepository masterRadioRepository, TelemetryClient telemetryClient)
        {
            _claimsProvider = claimsProvider;
            _tConService = tConService;
            _appTallyDbContext = appTallyDbContext;
            _systemLogQuery = systemLogQuery;
            _tconMasterRadioTenantMapRepository = tconMasterRadioTenantMapRepository;
            _aliveMonitorQuery = aliveMonitorQuery;
            _tallyMonitor = tallyMonitor;
            _masterRadioRepository = masterRadioRepository;
            _telemetryClient = telemetryClient;
        }

        [HttpPost]
        [Route("replace")]
        public async Task<IActionResult> ReplaceRadio(int radioAddressSourceId, int radioAddressDestinationId)
        {
            var sourceRadioCodes = await _tConService.FindAllRadioAccessCodesForRadio(radioAddressSourceId);
            foreach (var sourceRadioCode in sourceRadioCodes)
            {
                await _tConService.AddRadioAccessCodeIfNotAlreadyAdded(sourceRadioCode.Code, sourceRadioCode.IsKeyCode, sourceRadioCode.Option, radioAddressDestinationId);
                await _tConService.RemoveRadioAccessCodeFromSingleRadio(radioAddressSourceId, sourceRadioCode.Code);
            }

            await _appTallyDbContext.SaveChangesAsync();
            return new OkResult();
        }

        [HttpPost]
        [Route("copy")]
        public async Task<IActionResult> CopyRadio(int radioAddressSourceId, int radioAddressDestinationId)
        {
            var sourceRadioCodes = await _tConService.FindAllRadioAccessCodesForRadio(radioAddressSourceId);
            foreach (var sourceRadioCode in sourceRadioCodes)
            {
                await _tConService.AddRadioAccessCodeIfNotAlreadyAdded(sourceRadioCode.Code, sourceRadioCode.IsKeyCode, sourceRadioCode.Option, radioAddressDestinationId);
            }

            await _appTallyDbContext.SaveChangesAsync();
            return new OkResult();
        }

        [HttpGet]
        [Route("systemlog/list/all")]
        public async Task<ObjectResult> FindSystemLogs()
        {
            var masterRadioAddress = _tconMasterRadioTenantMapRepository.MasterRadioId;
            var systemLogDtos = await _systemLogQuery.ExecuteAsync(masterRadioAddress);
            return new ObjectResult(systemLogDtos);
        }

        [HttpGet]
        [Route("alivemonitor")]
        public async Task<ObjectResult> GetAliveMonitor()
        {
            var aliveMonitorDto = await _aliveMonitorQuery.ExecuteAsync();
            return new ObjectResult(aliveMonitorDto);
        }

        [HttpGet]
        [Route("performtconmonitoring")]
        public async Task<IActionResult> PerformTconMonitoring()
        {
            await _tallyMonitor.PerformTconMonitoring();
            return new OkResult();
        }

        [HttpPost]
        [Route("reset")]
        public async Task<IActionResult> Reset()
        {
            await _masterRadioRepository.PublishUpdate();
            // TODO: Implement cache clearing without Service Fabric
            // try
            // {
            //     var proxy = ServiceProxyHelper.CreateActorServiceProxy<IMemCacheActor>(0);
            //     await proxy.ClearCachedGates();
            // }
            // catch (Exception exception)
            // {
            //     _telemetryClient.TrackException(exception);
            // }
            return new OkResult();
        }

        [HttpGet]
        [Route("gates/string")]
        public async Task<string> GetGates()
        {
            // TODO: Implement gate retrieval without Service Fabric
            // try
            // {
            //     var proxy = ServiceProxyHelper.CreateActorServiceProxy<IMemCacheActor>(0);
            //     var gateOutRadios = (await proxy.GetCachedGates(MemCacheActorConstants.CACHED_GATES_OUT)).Where(x => x.TenantId == _claimsProvider.GetTenantId());
            //     var gateInRadios = (await proxy.GetCachedGates(MemCacheActorConstants.CACHED_GATES_IN)).Where(x => x.TenantId == _claimsProvider.GetTenantId());

            //     return "Gate-in: " + string.Join(',', gateInRadios.Select(x => x.RadioAddress)) + ", " + "Gate-out: " +
            //            string.Join(',', gateOutRadios.Select(x => x.RadioAddress));
            // }
            // catch (Exception)
            // {
                return "No gates loaded. Try again later";
            // }
        }
    }
}
