using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using NxB.Domain.Common.Constants;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.AllocationApi;
using NxB.Clients.Interfaces;
using NxB.Dto.PricingApi;
using NxB.BookingApi.Models;
// TODO: Replace Service Fabric MemCacheActor functionality
// using NxB.MemCacheActor.Interfaces;
using NxB.BookingApi.Infrastructure;

namespace NxB.BookingApi.Controllers.Pricing
{
    [Produces("application/json")]
    [Route("costinterval")]
    [Authorize]
    [ApiValidationFilter]
    public class CostIntervalController : BaseController
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly CostIntervalFactory _costIntervalFactory;
        private readonly ICostIntervalRepository _costIntervalRepository;
        private readonly TelemetryClient _telemetryClient;
        // TODO: Replace Service Fabric MemCacheActor functionality
        // private readonly IMemCacheActor _memCacheActor;
        private readonly IClaimsProvider _claimsProvider;

        public CostIntervalController(AppDbContext appDbContext, IMapper mapper, CostIntervalFactory costIntervalFactory, ICostIntervalRepository costIntervalRepository, TelemetryClient telemetryClient, IClaimsProvider claimsProvider)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _costIntervalFactory = costIntervalFactory;
            _costIntervalRepository = costIntervalRepository;
            _telemetryClient = telemetryClient;
            // TODO: Replace Service Fabric MemCacheActor functionality
            // _memCacheActor = memCacheActor;
            _claimsProvider = claimsProvider;
        }

        [HttpPost]
        [Route("")]
        public async Task<ObjectResult> CreateCostInterval([FromBody] CreateCostIntervalDetails createDto)
        {
            var costInterval = _costIntervalFactory.Create(createDto.CostType);
            _mapper.Map(createDto, costInterval);
            _costIntervalRepository.Add(costInterval);
            await _appDbContext.SaveChangesAsync();

            var costIntervalDto = _mapper.Map<CostIntervalDto>(costInterval);

            return new CreatedResult(new Uri("?id=" + costIntervalDto.Id, UriKind.Relative), costIntervalDto);
        }

        [HttpPost]
        [Route("list")]
        public async Task<ObjectResult> CreateOrModifyMultipleCostIntervals([FromBody] List<CreateOrModifyCostIntervalDetails> createOrModifyDto)
        {
            List<CostIntervalDto> costIntervalDtos = new();

            using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }, TransactionScopeAsyncFlowOption.Enabled);
            //use transactionScope top make sort (identity column) increment correctly
            foreach (var dto in createOrModifyDto)
            {
                CostInterval costInterval = null;
                if (dto.Action == "create")
                {
                    costInterval = _costIntervalFactory.Create(dto.CostType);
                    _costIntervalRepository.Add(costInterval);
                }
                else if (dto.Action == "modify")
                {
                    _costIntervalRepository.UpdateType(dto.Id.Value, dto.CostType);
                    costInterval = await _costIntervalRepository.FindSingleOrDefault(dto.Id.Value);
                    _costIntervalRepository.Update(costInterval);
                }
                else if (dto.Action == "delete")
                {
                    costInterval = await _costIntervalRepository.FindSingleOrDefault(dto.Id.Value);
                    _costIntervalRepository.DeletePermanently(costInterval);
                }

                if (dto.Action != "delete")
                {
                    dto.Id = costInterval.Id;
                    _mapper.Map(dto, costInterval);

                    var costIntervalDto = _mapper.Map<CostIntervalDto>(costInterval);
                    costIntervalDtos.Add(costIntervalDto);
                }
                await _appDbContext.SaveChangesAsync();
            }


            transactionScope.Complete();
            // TODO: Replace Service Fabric MemCacheActor functionality
            // await _memCacheActor.TryPublishCacheUpdated(GetTenantId(), _telemetryClient);
            // var modifiedPriceProfileIds = createOrModifyDto.Select(x => x.PriceProfileId).ToList();
            // await _memCacheActor.TryPublishPriceProfilesUpdated(GetTenantId(), modifiedPriceProfileIds, _telemetryClient);

            return new CreatedResult(new Uri("", UriKind.Relative), costIntervalDtos);
        }

        [HttpPost]
        [Route("list/all")]
        public async Task<ObjectResult> CreateCostIntervals([FromBody] List<CreateCostIntervalDetails> createDtos)
        {
            var createdDtos = new List<CostInterval>();

            foreach (var createDto in createDtos)
            {
                var costInterval = _costIntervalFactory.Create(createDto.CostType);
                _mapper.Map(createDto, costInterval);
                _costIntervalRepository.Add(costInterval);
                createdDtos.Add(costInterval);
            }

            var costIntervalDtos = createdDtos.Select(x => _mapper.Map<CostIntervalDto>(x));

            await _appDbContext.SaveChangesAsync();
            // TODO: Replace Service Fabric MemCacheActor functionality
            // await _memCacheActor.TryPublishCacheUpdated(GetTenantId(), _telemetryClient);

            return new ObjectResult(costIntervalDtos);
        }

        [HttpGet]
        [Route("")]
        public async Task<ObjectResult> FindSingle(Guid id)
        {
            var costInterval = await _costIntervalRepository.FindSingleOrDefault(id);
            var costIntervalDto = _mapper.Map<CostIntervalDto>(costInterval);
            return new ObjectResult(costIntervalDto);
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAll()
        {
            var costIntervals = await _costIntervalRepository.FindAll();
            var costIntervalDtos = costIntervals.Select(x => _mapper.Map<CostIntervalDto>(x));
            return new ObjectResult(costIntervalDtos);
        }
    }
}