using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.Clients;
using NxB.Dto.OrderingApi;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;
using NxB.Settings.Shared.Infrastructure;

namespace NxB.BookingApi.Controllers.Ordering
{
    [Produces("application/json")]
    [Route("allocationstate")]
    [Authorize]
    [ApiValidationFilter]
    public class AllocationStateController : BaseController
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAllocationStateRepository _allocationStateRepository;
        private readonly AllocationStateMapper _allocationStateMapper;
        private readonly TelemetryClient _telemetryClient;
        private readonly ISettingsRepository _settingsRepository;
        private readonly ICounterPushUpdateService _counterPushUpdateService;
        private readonly IGroupedBroadcasterClient _groupedBroadcasterClient;
        private readonly IMapper _mapper;
        private readonly IMemCacheActor _memCacheActor;

        public AllocationStateController(AppDbContext appDbContext, IAllocationStateRepository allocationStateRepository, AllocationStateMapper allocationStateMapper, TelemetryClient telemetryClient, ISettingsRepository settingsRepository, ICounterPushUpdateService counterPushUpdateService, IGroupedBroadcasterClient groupedBroadcasterClient, IMapper mapper, IMemCacheActor memCacheActor)
        {
            _appDbContext = appDbContext;
            _allocationStateRepository = allocationStateRepository;
            _allocationStateMapper = allocationStateMapper;
            _telemetryClient = telemetryClient;
            _settingsRepository = settingsRepository;
            _counterPushUpdateService = counterPushUpdateService;
            _groupedBroadcasterClient = groupedBroadcasterClient;
            _mapper = mapper;
            _memCacheActor = memCacheActor;
        }

        [HttpGet]
        [Route("")]
        public async Task<ObjectResult> FindSingleOrDefault(Guid subOrderId)
        {
            var allocationState = _allocationStateRepository.FindSingle(subOrderId);
            if (allocationState == null) return new ObjectResult(null);
            var allocationStateDto = _mapper.Map<AllocationStateDto>(allocationState);

            return new ObjectResult(allocationStateDto);
        }

        [HttpPost]
        [Route("arrival")]
        public async Task<ActionResult> AddArrivalState([FromBody] AddAllocationStateDto addAllocationStateDto)
        {
            var allocationState = _allocationStateRepository.FindSingle(addAllocationStateDto.SubOrderId);
            _allocationStateMapper.MapArrival(allocationState, addAllocationStateDto);
            
            var orgAllocationState = allocationState.ArrivalStatus;

            allocationState.DepartureStatus = DepartureStatus.NotDeparted;

            _allocationStateRepository.Update(allocationState);
            await _appDbContext.SaveChangesAsync();

            var createdResult = GetCreatedResult(allocationState.SubOrderId);

            await _counterPushUpdateService.TryPushUpdateMissingArrivalsCounter().CatchExceptionAndLogToTelemetry(_telemetryClient);
            await _counterPushUpdateService.TryPushUpdateMissingDeparturesCounter().CatchExceptionAndLogToTelemetry(_telemetryClient);
            _groupedBroadcasterClient.TryOrderModified(Guid.Empty).FireAndForgetLogToTelemetry(_telemetryClient);

            await _memCacheActor.PublishSubOrderArrivalStateChanged(GetTenantId(), allocationState.SubOrderId, orgAllocationState, allocationState.ArrivalStatus);

            return createdResult;
        }


        [HttpPost]
        [Route("departure")]
        public async Task<ActionResult> AddDepartureState([FromBody] AddAllocationStateDto addAllocationStateDto)
        {
            var allocationState = _allocationStateRepository.FindSingle(addAllocationStateDto.SubOrderId);
            _allocationStateMapper.MapDeparture(allocationState, addAllocationStateDto);

            var orgAllocationState = allocationState.DepartureStatus;

            if (allocationState.DepartureStatus == DepartureStatus.NotDeparted)
            {
                allocationState.ArrivalStatus = ArrivalStatus.Arrived;
            }
            _allocationStateRepository.Update(allocationState);
            await _appDbContext.SaveChangesAsync();

            var createdResult = GetCreatedResult(allocationState.SubOrderId);

            await _counterPushUpdateService.TryPushUpdateMissingArrivalsCounter().CatchExceptionAndLogToTelemetry(_telemetryClient);
            await _counterPushUpdateService.TryPushUpdateMissingDeparturesCounter().CatchExceptionAndLogToTelemetry(_telemetryClient);
            _groupedBroadcasterClient.TryOrderModified(Guid.Empty).FireAndForgetLogToTelemetry(_telemetryClient); ;

            await _memCacheActor.PublishSubOrderDepartureStateChanged(GetTenantId(), allocationState.SubOrderId, orgAllocationState, allocationState.DepartureStatus);

            return createdResult;
        }

        private CreatedResult GetCreatedResult(Guid subOrderId)
        {
            var allocationState = _allocationStateRepository.FindSingle(subOrderId);
            var dto = _allocationStateMapper.Map(allocationState);

            var createdResult = new CreatedResult(new Uri("?id=" + allocationState.SubOrderId, UriKind.Relative), dto);
            return createdResult;
        }
    }
}
