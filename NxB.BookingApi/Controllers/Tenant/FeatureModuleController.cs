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
using NxB.Dto.Clients;
using NxB.Dto.AllocationApi;

namespace NxB.BookingApi.Controllers.Tenant
{
    [Produces("application/json")]
    [Route("featuremodule")]
    [Authorize]
    [ApiValidationFilter]
    public class FeatureModuleController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly TelemetryClient _telemetry;
        private readonly AppDbContext _appDbContext;
        private readonly FeatureModuleFactory _featureModuleFactory;
        private readonly IFeatureModuleRepository _featureModuleRepository;

        public FeatureModuleController(IMapper mapper, TelemetryClient telemetry, AppDbContext appDbContext, FeatureModuleFactory featureModuleFactory, IFeatureModuleRepository featureModuleRepository)
        {
            _mapper = mapper;
            _telemetry = telemetry;
            _appDbContext = appDbContext;
            _featureModuleFactory = featureModuleFactory;
            _featureModuleRepository = featureModuleRepository;
        }

        [HttpPost]
        [Route("")]
        [Authorize(Roles = "Admin")]
        public async Task<ObjectResult> CreateFeatureModule([FromBody] CreateFeatureModuleDto dto)
        {
            var featureModule = _featureModuleFactory.Create(dto.Name);
            _mapper.Map(dto, featureModule);
            _featureModuleRepository.Add(featureModule);
            await _appDbContext.SaveChangesAsync();
            var featureModuleDto = _mapper.Map<FeatureModuleDto>(featureModule);
            return new CreatedResult("", featureModuleDto);
        }

        [HttpPut]
        [Route("")]
        [Authorize(Roles = "Admin")]
        public async Task<ObjectResult> ModifyFeatureModule([FromBody] ModifyFeatureModuleDto dto)
        {
            var featureModule = await _featureModuleRepository.FindSingleOrDefault(dto.Id);
            _mapper.Map(dto, featureModule);
            _featureModuleRepository.Update(featureModule);
            await _appDbContext.SaveChangesAsync();
            var featureModuleDto = _mapper.Map<FeatureModuleDto>(featureModule);
            return new OkObjectResult(featureModuleDto);
        }

        [HttpGet]
        [Route("")]
        public async Task<ObjectResult> FindSingleOrDefaultFeatureModule([NoEmpty] Guid id)
        {
            var featureModule = await _featureModuleRepository.FindSingleOrDefault(id);
            if (featureModule == null) return new OkObjectResult(null);
            var featureModuleDto = _mapper.Map<FeatureModule, FeatureModuleDto>(featureModule);
            return new OkObjectResult(featureModuleDto);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("tenant/isactive")]
        public async Task<bool> IsFeatureModuleActivatedForTenant([NoEmpty] Guid featureModuleId, [NoEmpty] Guid tenantId)
        {
            var featureModule = await _featureModuleRepository.FindLastFeatureModuleEntryForFeatureModuleFromTenantId(featureModuleId, tenantId);
            if (featureModule == null) return false;
            return featureModule.IsActive;
        }

        [HttpGet]
        [Route("isactive")]
        public async Task<bool> IsFeatureModuleActivated([NoEmpty] Guid featureModuleId)
        {
            var featureModule = await _featureModuleRepository.FindLastFeatureModuleEntryForFeatureModule(featureModuleId);
            if (featureModule == null) return false;
            return featureModule.IsActive;
        }

        [HttpGet]
        [Route("isdeactivated")]
        public async Task<bool> IsFeatureModuleDeactivated([NoEmpty] Guid featureModuleId)
        {
            var featureModule = await _featureModuleRepository.FindLastFeatureModuleEntryForFeatureModule(featureModuleId);
            if (featureModule == null) return false;
            return !featureModule.IsActive;
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAllFeatureModules()
        {
            var featureModules = await _featureModuleRepository.FindAll(false);
            var featureModuleDtos = _mapper.Map<List<FeatureModuleDto>>(featureModules);
            return new OkObjectResult(featureModuleDtos);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        [Route("markdeleted")]
        public async Task<ObjectResult> MarkDeleted([NoEmpty] Guid id, bool isDeleted = true)
        {
            var featureModule = await _featureModuleRepository.FindSingleOrDefault(id);
            if (featureModule == null) return new NotFoundObjectResult(null);

            if (isDeleted)
            {
                await _featureModuleRepository.MarkAsDeleted(id);
            }
            else
            {
                await _featureModuleRepository.MarkAsUndeleted(id);
            }

            await _appDbContext.SaveChangesAsync();

            var featureModuleDto = _mapper.Map<FeatureModuleDto>(featureModule);
            return new ObjectResult(featureModuleDto);
        }
    }
}