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
using NxB.BookingApi.Infrastructure;
using NxB.Settings.Shared.Infrastructure;
using NxB.Dto.TenantApi;
using NxB.BookingApi.Models;
using Munk.Utils.Object;
using NxB.Clients.Interfaces;
using NxB.Dto.AllocationApi;

namespace NxB.BookingApi.Controllers.Tenant
{
    [Produces("application/json")]
    [Route("featuremoduletenantentry")]
    [Authorize]
    [ApiValidationFilter]
    public class FeatureModuleTenantEntryController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly TelemetryClient _telemetry;
        private readonly AppDbContext _appDbContext;
        private readonly FeatureModuleFactory _featureModuleFactory;
        private readonly IFeatureModuleRepository _featureModuleRepository;
        private readonly IFeatureModuleService _featureModuleService;
        private readonly IRentalUnitClient _rentalUnitClient;
        private readonly ITenantClient _tenantClient;

        public FeatureModuleTenantEntryController(IMapper mapper, TelemetryClient telemetry, AppDbContext appDbContext, FeatureModuleFactory featureModuleFactory, IFeatureModuleRepository featureModuleRepository, IFeatureModuleService featureModuleService, IRentalUnitClient rentalUnitClient, ITenantClient tenantClient)
        {
            _mapper = mapper;
            _telemetry = telemetry;
            _appDbContext = appDbContext;
            _featureModuleFactory = featureModuleFactory;
            _featureModuleRepository = featureModuleRepository;
            _featureModuleService = featureModuleService;
            _rentalUnitClient = rentalUnitClient;
            _tenantClient = tenantClient;
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAllFeatureModuleEntries()
        {
            var featureModuleEntries = await _featureModuleRepository.FindAllFeatureModuleEntries();
            var featureModuleDtos = _mapper.Map<List<FeatureModuleTenantEntryDto>>(featureModuleEntries);
            if (featureModuleDtos.Any())
            {
                featureModuleDtos.First().IsActiveEntry = true;
            }
            return new OkObjectResult(featureModuleDtos);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("list/all/tenants")]
        public async Task<ObjectResult> FindAllFeatureModuleEntriesForAllTenants()
        {
            var tenants = await _tenantClient.FindActiveTenants();
            var featureModuleEntries = await _featureModuleRepository.FindAllFeatureModuleEntriesForAllTenants();
            var featureModuleDtos = _mapper.Map<List<FeatureModuleTenantEntryDto>>(featureModuleEntries);

            var groupedByTenant = featureModuleDtos.GroupBy(x => x.TenantId);
            foreach (var group in groupedByTenant)
            {
                group.First().IsActiveEntry = true;
            }

            featureModuleDtos.ForEach(x =>
            {
                var tenant = tenants.FirstOrDefault(t => t.Id == x.TenantId);
                if (tenant != null)
                {
                    x.TenantName = tenant.CompanyName;
                }
            });

            return new OkObjectResult(featureModuleDtos);
        }

        [HttpGet]
        [Route("activate/pseudo")]
        public async Task<ObjectResult> ActivatePseudo([NoEmpty] Guid id)
        {
            var activationEntry = await ActivateFeatureModule(new ActivateFeatureModuleDto{FeatureModuleId = id});
            var featureModuleDtos = _mapper.Map<FeatureModuleTenantEntryDto>(activationEntry);
            return new OkObjectResult(featureModuleDtos);
        }

        [HttpPost]
        [Route("activatetrial")]
        public async Task<ObjectResult> ActivateTrial([FromBody] ActivateFeatureModuleDto dto)
        {
            var featureModule = await _featureModuleRepository.FindSingleOrDefault(dto.FeatureModuleId);
            if (featureModule == null) throw new ArgumentNullException(nameof(featureModule));
            var featureModuleTenantEntries = await _featureModuleRepository.FindAllFeatureModuleEntriesForFeatureModule(dto.FeatureModuleId);
            var activationEntry = await _featureModuleService.ActivateModuleForTrial(GetTenantId(), GetUserId(), featureModule, featureModuleTenantEntries);
            _featureModuleRepository.Add(activationEntry);
            await _appDbContext.SaveChangesAsync();
            var activationEntryDto = _mapper.Map<FeatureModuleTenantEntryDto>(activationEntry);
            return new CreatedResult("", activationEntryDto);
        }

        [HttpPost]
        [Route("activate")]
        public async Task<ObjectResult> Activate([FromBody] ActivateFeatureModuleDto dto)
        {
            var activationEntry = await ActivateFeatureModule(dto);
            _featureModuleRepository.Add(activationEntry);
            await _appDbContext.SaveChangesAsync();
            var activationEntryDto = _mapper.Map<FeatureModuleTenantEntryDto>(activationEntry);
            return new CreatedResult("", activationEntryDto);
        }

        private async Task<FeatureModuleTenantEntry> ActivateFeatureModule(ActivateFeatureModuleDto dto)
        {
            var featureModule = await _featureModuleRepository.FindSingleOrDefault(dto.FeatureModuleId);
            if (featureModule == null) throw new ArgumentNullException(nameof(featureModule));
            var featureModuleTenantEntries = await _featureModuleRepository.FindAllFeatureModuleEntriesForFeatureModule(dto.FeatureModuleId);
            var rentalUnits = await _rentalUnitClient.FindAll(false);
            var activationEntry = await _featureModuleService.ActivateModule(GetTenantId(), GetUserId(), featureModule,
                featureModuleTenantEntries, rentalUnits.Count);
            return activationEntry;
        }

        [HttpPost]
        [Route("deactivate")]
        public async Task<ObjectResult> Deactivate([FromBody] ActivateFeatureModuleDto dto)
        {
            var featureModule = await _featureModuleRepository.FindSingleOrDefault(dto.FeatureModuleId);
            if (featureModule == null) throw new ArgumentNullException(nameof(featureModule));
            var featureModuleTenantEntries = await _featureModuleRepository.FindAllFeatureModuleEntriesForFeatureModule(dto.FeatureModuleId);
            var deactivationEntry = await _featureModuleService.DeactivateModule(GetTenantId(), GetUserId(), featureModule, featureModuleTenantEntries);
            _featureModuleRepository.Add(deactivationEntry);
            await _appDbContext.SaveChangesAsync();
            var activationEntryDto = _mapper.Map<FeatureModuleTenantEntryDto>(deactivationEntry);
            return new CreatedResult("", activationEntryDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("renew")]
        public async Task<ObjectResult> Renew([NoEmpty] Guid featureModuleEntryId)
        {
            var featureModuleTenantEntry = await _featureModuleRepository.FindSingleOrDefaultEntry(featureModuleEntryId);
            if (featureModuleTenantEntry == null) throw new ArgumentNullException(nameof(featureModuleTenantEntry));
            var renewedEntry = await _featureModuleService.Renew(GetTenantId(), GetUserId(), featureModuleTenantEntry);
            _featureModuleRepository.Update(featureModuleTenantEntry);
            _featureModuleRepository.Add(renewedEntry);
            await _appDbContext.SaveChangesAsync();
            var renewedEntryDto = _mapper.Map<FeatureModuleTenantEntryDto>(renewedEntry);
            return new CreatedResult("", renewedEntryDto);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        [Route("")]
        public async Task<ObjectResult> Modify([FromBody] ModifyAdminFeatureModuleTenantEntryDto dto)
        {
            var featureModule = await _featureModuleRepository.FindSingleOrDefaultEntry(dto.Id);
            if (featureModule == null) throw new ArgumentNullException(nameof(featureModule));

            _mapper.Map(dto, featureModule);
            await _appDbContext.SaveChangesAsync();

            var featureModuleDto = _mapper.Map<FeatureModuleTenantEntryDto>(featureModule);
            return new ObjectResult(featureModuleDto);
        }
    }
}