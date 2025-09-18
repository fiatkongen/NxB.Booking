using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using NxB.Dto.TallyWebIntegrationApi;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Controllers.TallyWebIntegration
{
    [Produces("application/json")]
    [Route("switch")]
    [Authorize]
    [ApiValidationFilter]
    public class SwitchController : BaseController
    {
        private readonly ITConService _tconService;
        private readonly IMapper _mapper;

        public SwitchController(ITConService tconService, IMapper mapper)
        {
            _tconService = tconService;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAllSwitches()
        {
            var switches = await _tconService.FindAllSwitches();
            var switchDtos = switches.Select(x => _mapper.Map<SwitchDto>(x)).ToList();
            return new ObjectResult(switchDtos);
        }

        [HttpPut]
        [Route("lock")]
        public async Task<ObjectResult> LockSwitch(int radioAddress)
        {
            await _tconService.LockSwitch(radioAddress);
            var @switch = await _tconService.FindSwitch(radioAddress);
            var switchDto = _mapper.Map<SwitchDto>(@switch);
            return new ObjectResult(switchDto);
        }

        [HttpPut]
        [Route("unlock")]
        public async Task<ObjectResult> UnlockSwitch(int radioAddress)
        {
            await _tconService.UnlockSwitch(radioAddress);
            var @switch = await _tconService.FindSwitch(radioAddress);
            var switchDto = _mapper.Map<SwitchDto>(@switch);
            return new ObjectResult(switchDto);
        }

        [HttpPut]
        [Route("open")]
        public async Task<ObjectResult> OpenSwitch(int radioAddress, int pulseTime, bool force = false)
        {
            await _tconService.OpenSwitch(radioAddress, pulseTime, force);
            var @switch = await _tconService.FindSwitch(radioAddress);
            var switchDto = _mapper.Map<SwitchDto>(@switch);
            return new ObjectResult(switchDto);
        }

        [HttpPut]
        [Route("close")]
        public async Task<ObjectResult> CloseSwitch(int radioAddress, bool force = false)
        {
            await _tconService.CloseSwitch(radioAddress, force);
            var @switch = await _tconService.FindSwitch(radioAddress);
            var switchDto = _mapper.Map<SwitchDto>(@switch);
            return new ObjectResult(switchDto);
        }

        [HttpPut]
        [Route("name")]
        public async Task<ObjectResult> ModifyName(int radioAddress, string name)
        {
            await _tconService.ModifySwitchName(radioAddress, name);
            var @switch = await _tconService.FindSwitch(radioAddress);
            var statusDto = _mapper.Map<SwitchDto>(@switch);
            return new ObjectResult(statusDto);
        }

        [HttpPut]
        [Route("pulsetimer")]
        public async Task<ObjectResult> ModifyPulseSetup(int radioAddress, int pulseTimer)
        {
            await _tconService.ModifySwitchPulseTimer(radioAddress, pulseTimer);
            var @switch = await _tconService.FindSwitch(radioAddress);
            var switchDto = _mapper.Map<SwitchDto>(@switch);
            return new ObjectResult(switchDto);
        }

        [HttpGet]
        [Route("log/list")]
        public async Task<ObjectResult> FindLogs(int radioAddress, int takeCount = 1000)
        {
            var switchLogs = await _tconService.FindSwitchLogs(radioAddress, takeCount);
            var switchLogDtos = switchLogs.Select(x => _mapper.Map<SwitchLogDto>(x)).ToList();
            return new ObjectResult(switchLogDtos);
        }
    }
}
