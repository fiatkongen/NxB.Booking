using AutoMapper;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NxB.Dto.TenantApi;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;
using NxB.Settings.Shared.Infrastructure;

namespace NxB.BookingApi.Controllers.Tenant
{
    [Produces("application/json")]
    [Authorize]
    [Route("kiosk")]
    [ApiValidationFilter]
    public class KioskController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly TelemetryClient _telemetry;
        private readonly AppDbContext _appDbContext;
        private readonly KioskFactory _kioskFactory;
        private readonly IKioskRepository _kioskRepository;
        private readonly ISettingsRepository _settingsRepository;

        public KioskController(IMapper mapper, TelemetryClient telemetry, AppDbContext appDbContext, KioskFactory kioskFactory, IKioskRepository kioskRepository, ISettingsRepository settingsRepository)
        {
            _mapper = mapper;
            _telemetry = telemetry;
            _appDbContext = appDbContext;
            _kioskFactory = kioskFactory;
            _kioskRepository = kioskRepository;
            _settingsRepository = settingsRepository;
        }

        [HttpPost]
        [Route("")]
        [Authorize(Roles = "Admin")]
        public async Task<ObjectResult> CreateKiosk([FromBody] CreateKioskDto dto)
        {
            var kiosk =_kioskFactory.Create(dto.HardwareSerialNo, dto.Name);
            _mapper.Map(dto, kiosk);
            _kioskRepository.Add(kiosk);
            await _appDbContext.SaveChangesAsync();
            var kioskDto = _mapper.Map<KioskDto>(kiosk);
            return new CreatedResult("", kioskDto);
        }

        [HttpPut]
        [Route("")]
        public async Task<ObjectResult> ModifyKiosk([FromBody] ModifyKioskDto dto)
        {
            var kiosk = await _kioskRepository.FindSingleOrDefault(dto.Id);
            _mapper.Map(dto, kiosk);
            _kioskRepository.Update(kiosk);
            await _appDbContext.SaveChangesAsync();
            var kioskDto = _mapper.Map<KioskDto>(kiosk);
            return new ObjectResult(kioskDto);
        }

        [HttpPut]
        [AllowAnonymous]
        [Route("synchronize")]
        public async Task<ObjectResult> SynchronizeKiosk(Guid id)
        {

            var kiosk = await _kioskRepository.FindSingleOrDefault(id);
            kiosk.UpdateOnlineStatus();
            _kioskRepository.Update(kiosk);
            await _appDbContext.SaveChangesAsync();
            var kioskDto = _mapper.Map<KioskDto>(kiosk);
            return new ObjectResult(kioskDto);
        }

        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public async Task<ObjectResult> FindSingleKiosk(Guid id)
        {
            var kiosk = await _kioskRepository.FindSingleOrDefault(id);
            if (kiosk == null) return new ObjectResult(null);
            var kioskDto = _mapper.Map<KioskDto>(kiosk);
            return new ObjectResult(kioskDto);
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteKiosk(Guid id)
        {
            var kiosk = await _kioskRepository.FindSingleOrDefault(id);
            _appDbContext.Remove(kiosk);
            await _appDbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("serialno")]
        public async Task<ObjectResult> FindSingleKiosk(string serialNo)
        {
            var kiosk = await _kioskRepository.FindSingleOrDefaultFromHardwareSerialNo(serialNo);
            if (kiosk == null) return new ObjectResult(null);
            var kioskDto = _mapper.Map<KioskDto>(kiosk);
            return new ObjectResult(kioskDto);
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAll(bool includeDeleted = false)
        {
            var kiosks = await _kioskRepository.FindAll(includeDeleted);
            var kioskDto = kiosks.Select(x => _mapper.Map<KioskDto>(x));
            return new ObjectResult(kioskDto);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("onlinesettings")]
        public ObjectResult FindSettings(Guid tenantId)
        {
            var jsonResult = this._settingsRepository.GetKioskOnlineSettings(tenantId);
            return new ObjectResult(jsonResult);
        }
    }
}