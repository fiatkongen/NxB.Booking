using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using NxB.Domain.Common.Enums;
using NxB.Dto.TallyWebIntegrationApi;
// TODO: Remove Service Fabric dependencies when migration is complete
// using NxB.MemCacheActor.Interfaces;
// using SchedulingActor.Interfaces;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;


namespace NxB.BookingApi.Controllers.TallyWebIntegration
{
    [Produces("application/json")]
    [Route("socketswitchcontroller")]
    [Authorize]
    [ApiValidationFilter]
    public class SocketSwitchControllerController : BaseController
    {
        private readonly ITConService _tconService;
        private readonly IMapper _mapper;
        private readonly IMasterRadioRepository _masterRadioRepository;
        private readonly TelemetryClient _telemetryClient;

        public SocketSwitchControllerController(ITConService tconService, IMapper mapper, IMasterRadioRepository masterRadioRepository, TelemetryClient telemetryClient)
        {
            _tconService = tconService;
            _mapper = mapper;
            _masterRadioRepository = masterRadioRepository;
            _telemetryClient = telemetryClient;
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAllSocketSwitchControllers()
        {
            var socketSwitchControllers = await _tconService.FindAllSocketSwitchControllers(null);
            var socketSwitchControllerDtos = socketSwitchControllers.Select(x => _mapper.Map<SocketSwitchControllerDto>(x)).ToList();
            return new ObjectResult(socketSwitchControllerDtos);
        }

        [HttpPut]
        [Route("modus")]
        public async Task<IActionResult> SetSocketSwitchControllerModus(int radioAddress, TWIModus modus)
        {
            await _tconService.SetSocketSwitchControllerModus(radioAddress, modus);
            Thread.Sleep(4000);
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
            return Ok();
        }

        [HttpPut]
        [Route("keypad2")]
        public async Task<IActionResult> SetSocketSwitchControllerKeypad2Enabled(int radioAddress, bool enabled)
        {
            await _tconService.SetSocketSwitchControllerKeypad2Enabled(radioAddress, enabled);
            return Ok();
        }
    }
}
