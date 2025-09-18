using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using Newtonsoft.Json;
using NxB.Dto.TallyWebIntegrationApi;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Controllers.TallyWebIntegration
{
    [Produces("application/json")]
    [Route("socket")]
    [Authorize]
    [ApiValidationFilter]
    public class SocketController : BaseController
    {
        private readonly ITConService _tconService;
        private readonly IMapper _mapper;
        private readonly AppTallyDbContext _appTallyDbContext;

        public SocketController(ITConService tconService, IMapper mapper, AppTallyDbContext appTallyDbContext)
        {
            _tconService = tconService;
            _mapper = mapper;
            _appTallyDbContext = appTallyDbContext;
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAllSockets()
        {
            var sockets = await _tconService.FindAllSockets();
            var socketDtos = sockets.Select(x => _mapper.Map<SocketDto>(x)).ToList();
            return new ObjectResult(socketDtos);
        }

        [HttpGet]
        [Route("")]
        public async Task<ObjectResult> FindSingleSocket(int radioAddress, int socketNo)
        {
            var socket = await _tconService.FindSingleSocket(radioAddress, socketNo);
            var socketDto = _mapper.Map<SocketDto>(socket);
            return new ObjectResult(socketDto);
        }

        [HttpPut]
        [Route("open")]
        public async Task<ObjectResult> OpenSocket(int radioAddress, int socketNo, bool force = false)
        {
            try
            {
                await _tconService.OpenSocket(radioAddress, socketNo, force);
            }
            catch (Exception socketException)
            {
                return new BadRequestObjectResult(socketException.Message);
            }
            var socket = await _tconService.FindSocketBase(radioAddress, socketNo);
            var socketDto = _mapper.Map<SocketDto>(socket);
            await _appTallyDbContext.SaveChangesAsync();
            return new ObjectResult(socketDto);
        }

        [HttpPut]
        [Route("close")]
        public async Task<ObjectResult> CloseSocket(int radioAddress, int socketNo, bool force = false)
        {
            try
            {
                await _tconService.CloseSocket(radioAddress, socketNo, force);
            }
            catch (SocketException socketException)
            {
                return new BadRequestObjectResult(socketException.Message);
            }
            var socket = await _tconService.FindSocketBase(radioAddress, socketNo);
            var socketDto = _mapper.Map<SocketDto>(socket);
            await _appTallyDbContext.SaveChangesAsync();
            return new ObjectResult(socketDto);
        }

        [HttpPut]
        [Route("enable")]
        public async Task<ObjectResult> EnableSocket(int radioAddress, int socketNo, bool force = false)
        {
            try
            {
                await _tconService.EnableSocket(radioAddress, socketNo, force);
            }
            catch (SocketException socketException)
            {
                return new BadRequestObjectResult(socketException.Message);
            }
            var socket = await _tconService.FindSocketBase(radioAddress, socketNo);
            var socketDto = _mapper.Map<SocketDto>(socket);
            return new ObjectResult(socketDto);
        }

        [HttpPut]
        [Route("disable")]
        public async Task<ObjectResult> DisableSocket(int radioAddress, int socketNo, bool force = false)
        {
            try
            {
                await _tconService.DisableSocket(radioAddress, socketNo, force);
            }
            catch (SocketException socketException)
            {
                return new BadRequestObjectResult(socketException.Message);
            }

            var socket = await _tconService.FindSocketBase(radioAddress, socketNo);
            var socketDto = _mapper.Map<SocketDto>(socket);
            return new ObjectResult(socketDto);
        }

        [HttpPut]
        [Route("name")]
        public async Task<ObjectResult> ModifyName(int radioAddress, int socketNo, string name)
        {
            await _tconService.ModifySocketName(radioAddress, socketNo, name);
            var socket = await _tconService.FindSocketBase(radioAddress, socketNo);
            var socketDto = _mapper.Map<SocketDto>(socket);
            return new ObjectResult(socketDto);
        }

        [HttpPut]
        [Route("pulsesetup")]
        public async Task<ObjectResult> ModifyPulseSetup(int radioAddress, int socketNo, double pulseSetup)
        {
            await _tconService.ModifySocketPulseSetup(radioAddress, socketNo, pulseSetup);
            var socket = await _tconService.FindSocketBase(radioAddress, socketNo);
            var socketDto = _mapper.Map<SocketDto>(socket);
            return new ObjectResult(socketDto);
        }

        [HttpPut]
        [Route("consumption")]
        public async Task<ObjectResult> ModifyConsumed(int radioAddress, int socketNo, double consumption)
        {
            await _tconService.ModifySocketConsumption(radioAddress, socketNo, consumption);
            var socket = await _tconService.FindSocketBase(radioAddress, socketNo);
            var socketDto = _mapper.Map<SocketDto>(socket);
            return new ObjectResult(socketDto);
        }

        [HttpGet]
        [Route("consumption/list")]
        public async Task<ObjectResult> FindChargeConsumptions(int radioAddress, int socketNo)
        {
            var chargeConsumptions = await _tconService.FindChargeConsumptions(radioAddress, socketNo);
            var chargeConsumptionDtos = chargeConsumptions.Select(x => _mapper.Map<ChargeConsumptionDto>(x)).ToList();
            return new ObjectResult(chargeConsumptionDtos);
        }
    }
}
