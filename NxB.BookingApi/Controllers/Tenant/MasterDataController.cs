using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using NxB.Settings.Shared.Infrastructure;
using NxB.BookingApi.Infrastructure;

namespace NxB.BookingApi.Controllers.Tenant
{
    [Produces("application/json")]
    [Authorize]
    [Route("masterdata")]
    [ApiValidationFilter]
    public  class MasterDataController : BaseController
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly AppDbContext _appDbContext;

        public MasterDataController(ISettingsRepository settingsRepository, AppDbContext appDbContext)
        {
            _settingsRepository = settingsRepository;
            _appDbContext = appDbContext;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("online")]
        public IActionResult FindSettings(Guid tenantId)
        {
            var jsonResult = this._settingsRepository.GetOnlineBookingMasterDataJson(tenantId);
            return Content(jsonResult, "application/json");
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("online/searchsettings")]
        public ObjectResult FindSearchSettings(Guid tenantId)
        {
            var onlineSearchSettings = this._settingsRepository.GetOnlineBookingSearchSettings(tenantId);
            return new ObjectResult(onlineSearchSettings);
        }

    }
}