using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using NxB.Settings.Shared.Infrastructure;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Controllers.Tenant
{
    [Produces("application/json")]
    [Authorize]
    [Route("tenant/settings")]
    public class TenantSettingsController : Controller
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly AppDbContext _appDbContext;
        private readonly ITenantRepository _tenantRepository;

        public TenantSettingsController(ISettingsRepository settingsRepository, AppDbContext appDbContext, ITenantRepository tenantRepository)
        {
            _settingsRepository = settingsRepository;
            _appDbContext = appDbContext;
            _tenantRepository = tenantRepository;
        }

        [HttpPost]
        [Route("")]
        public ObjectResult SetSettings([FromBody]JObject jsonSettings, [FromQuery] string path = null)
        {
            this._settingsRepository.SetTenantJsonSettings(jsonSettings, path);
            _appDbContext.SaveChanges();
            var settings = this._settingsRepository.GetTenantJsonSettings(null);

            return new OkObjectResult(settings);
        }

        [HttpGet]
        [Route("")]
        public ObjectResult FindSettings(string path = null)
        {
            var settings = this._settingsRepository.GetTenantJsonSettings(path);
            return new OkObjectResult(settings);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("tenant/online/languages")]
        public ObjectResult FindBookingOnlineLanguages(Guid tenantId)
        {
            var languages = this._settingsRepository.GetOnlineBookingLanguages(tenantId);
            return new OkObjectResult(languages);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("tenant/guestinfo/languages")]
        public ObjectResult FindGuestInfoLanguages(Guid tenantId)
        {
            var languages = this._settingsRepository.GetGuestInfoLanguages(tenantId);
            return new OkObjectResult(languages);
        }


        [HttpGet]
        [AllowAnonymous]
        [Route("tenant/online/settings")]
        public ObjectResult FindBookingOnlineSettings(Guid tenantId)
        {
            var settings = this._settingsRepository.GetOnlineBookingSettings(tenantId);
            return new OkObjectResult(settings);
        }


        [HttpGet]
        [AllowAnonymous]
        [Route("tenant/online/contactinfosettings")]
        public ObjectResult FindOnlineContactInfoSettings(Guid tenantId)
        {
            var onlineContactInfoSettings = this._settingsRepository.GetOnlineContactInfoSettings(tenantId);
            return new ObjectResult(onlineContactInfoSettings);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("tenant/kiosk/contactinfosettings")]
        public ObjectResult FindKioskContactInfoSettings(Guid tenantId)
        {
            var kioskContactInfoSettings = this._settingsRepository.GetKioskContactInfoSettings(tenantId);
            return new ObjectResult(kioskContactInfoSettings);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("tenant/automationsettings")]
        public ObjectResult FindAutomationSettings(Guid tenantId)
        {
            var automationSettingsDto = this._settingsRepository.GetAutomationSettings(tenantId);
            automationSettingsDto.BaseUrl = null;
            automationSettingsDto.Login = null;
            automationSettingsDto.Password = null;
            return new ObjectResult(automationSettingsDto);
        }

        //this method is not tested
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("tenantids/all")]
        public async Task<ObjectResult> FindTenantIdsWithSetting(string settingsPath, bool? value = null)
        {
            var tenants = await _tenantRepository.FindAllActive();
            await _settingsRepository.CacheAllActiveTenantsSettings();
            List<Guid> tenantIds = new List<Guid>();

            foreach (var tenant in tenants)
            {
                var automationSettingsDto = this._settingsRepository.GetTenantJsonSettings(settingsPath, tenant.Id);
                //if (automationSettingsDto.IsOutletAutomationEnabled && !automationSettingsDto.UseLegacyTallyBee)
                //{

                //}
            }

            return new ObjectResult(tenantIds);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("tenantids/all/outletautomationenabled")]
        public async Task<ObjectResult> FindTenantIdsWithOutletAutomationEnabled()
        {
            var tenants = await _tenantRepository.FindAllActive();
            await _settingsRepository.CacheAllActiveTenantsSettings();
            Dictionary<Guid, (string, int)> tenantIds = new Dictionary<Guid, (string, int)>();

            foreach (var tenant in tenants)
            {
                var automationSettingsDto = this._settingsRepository.GetAutomationSettings(tenant.Id);
                if (automationSettingsDto.IsOutletAutomationEnabled && !automationSettingsDto.UseLegacyTallyBee && automationSettingsDto.OutletAutomationSynchronizeSeconds > 9)
                {
                    tenantIds.Add(tenant.Id, (tenant.CompanyName, automationSettingsDto.OutletAutomationSynchronizeSeconds));
                }
            }

            return new ObjectResult(tenantIds);
        }

        [HttpDelete]
        [Route("")]
        public ObjectResult DeleteSettings(string path)
        {
            this._settingsRepository.DeleteTenantJsonSettings(path);
            _appDbContext.SaveChanges();
            var settings = this._settingsRepository.GetTenantJsonSettings(null);

            return new OkObjectResult(settings);
        }
    }
}