using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using Munk.Utils.Object;
using NxB.Domain.Common.Constants;
using NxB.Domain.Common.Dto;
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
    [Route("priceprofile")]
    [Authorize]
    [ApiValidationFilter]
    public class PriceProfileController : BaseController
    {
        private readonly AppDbContext _appDbContext;
        private readonly IPriceProfileRepository _priceProfileRepository;
        private readonly PriceProfileFactory _priceProfileFactory;
        private readonly IAllocationApiClient _allocationApiClient;
        private readonly IRentalCategoryClient _rentalCategoryClient;
        private readonly IGuestCategoryClient _guestCategoryClient;
        private readonly IArticleClient _articleClient;
        private readonly IMapper _mapper;
        // TODO: Replace Service Fabric MemCacheActor functionality
        // private readonly IMemCacheActor _memCacheActor;
        private readonly TelemetryClient _telemetryClient;
        private readonly IClaimsProvider _claimsProvider;
        private readonly ICostIntervalRepository _costIntervalRepository;
        private readonly IPriceCalculationClient _priceCalculationClient;

        public PriceProfileController(AppDbContext appDbContext, IPriceProfileRepository priceProfileRepository, PriceProfileFactory priceProfileFactory, IAllocationApiClient allocationApiClient, IArticleClient articleClient, IMapper mapper, TelemetryClient telemetryClient, IClaimsProvider claimsProvider, IRentalCategoryClient rentalCategoryClient, IGuestCategoryClient guestCategoryClient, ICostIntervalRepository costIntervalRepository, IPriceCalculationClient priceCalculationClient)
        {
            _appDbContext = appDbContext;
            _priceProfileRepository = priceProfileRepository;
            _priceProfileFactory = priceProfileFactory;
            _allocationApiClient = allocationApiClient;
            _articleClient = articleClient;
            _mapper = mapper;
            // TODO: Replace Service Fabric MemCacheActor functionality
            // _memCacheActor = memCacheActor;
            _telemetryClient = telemetryClient;
            _claimsProvider = claimsProvider;
            _rentalCategoryClient = rentalCategoryClient;
            _guestCategoryClient = guestCategoryClient;
            _costIntervalRepository = costIntervalRepository;
            _priceCalculationClient = priceCalculationClient;
        }

        [HttpPost]
        [Route("")]
        public async Task<ObjectResult> CreatePriceProfile([FromBody] CreatePriceProfileDetails createPriceProfileDto)
        {
            var priceProfile = _priceProfileFactory.CreatePricePriceProfile(createPriceProfileDto.ResourceId, createPriceProfileDto.Name.ToLower(), createPriceProfileDto.FixedPrice);
            _priceProfileRepository.Add(priceProfile);
            await _appDbContext.SaveChangesAsync();

            var priceProfileDto = _mapper.Map<PriceProfileDto>(priceProfile);

            await TryPublishCacheUpdated();

            return new CreatedResult(new Uri("?id=" + priceProfileDto.Id, UriKind.Relative), priceProfileDto);
        }

        [HttpPost]
        [Route("copy")]
        public async Task<ObjectResult> CopyPriceProfile([FromBody] CopyPriceProfile copyPriceProfileDto)
        {
            GuestCategoryDto guestCategoryDto = null;

            var rentalCategory = await _rentalCategoryClient.FindSingleOrDefault(copyPriceProfileDto.DestinationResourceId);
            if (rentalCategory == null)
            {
                guestCategoryDto = await _guestCategoryClient.FindSingleOrDefault(copyPriceProfileDto.DestinationResourceId);
            }

            if (rentalCategory == null && guestCategoryDto == null)
                throw new ArgumentException("Could not find rentalCategory or guestCategory with id " +
                                            copyPriceProfileDto.DestinationResourceId);

            var sourcePriceProfile = _priceProfileRepository.FindSingle(copyPriceProfileDto.SourcePriceProfileId);

            if (sourcePriceProfile == null)
                throw new ArgumentException("Could not find priceprofile with id " +
                                            copyPriceProfileDto.SourcePriceProfileId);

            var priceProfile = _priceProfileFactory.CreatePricePriceProfile(guestCategoryDto?.Id ?? rentalCategory.Id, sourcePriceProfile.Name.ToLower(), sourcePriceProfile.FixedPrice);
            _priceProfileRepository.Add(priceProfile);

            priceProfile.Name = "kopi af " + priceProfile.Name;

            var costIntervals = await _costIntervalRepository.FindAllPriceFromPriceProfileId(copyPriceProfileDto.SourcePriceProfileId);
            foreach (var costInterval in costIntervals)
            {
                var clone = costInterval.Clone(priceProfile.Id);
                _costIntervalRepository.Add(clone);
            }

            await _appDbContext.SaveChangesAsync();

            var priceProfileDto = _mapper.Map<PriceProfileDto>(priceProfile);

            await TryPublishCacheUpdated();

            return new CreatedResult(new Uri("?id=" + priceProfileDto.Id, UriKind.Relative), priceProfileDto);
        }

        [HttpPut]
        [Route("rename")]
        public async Task<IActionResult> RenamePriceProfile([NoEmpty] Guid priceProfileId, string newName)
        {
            var priceProfile = _priceProfileRepository.FindSingle(priceProfileId);
            priceProfile.Name = newName.ToLower();
            await _appDbContext.SaveChangesAsync();
            await TryPublishCacheUpdated();
            return Ok();
        }

        [HttpPut]
        [Route("statistics")]
        public async Task<ObjectResult> ModifyStatisticsPriceProfile([FromBody] ModifyPriceProfileStatistics modifyPriceProfileDto)
        {
            var priceProfile = _priceProfileRepository.FindSingle(modifyPriceProfileDto.Id);
            _mapper.Map(modifyPriceProfileDto, priceProfile);
            _priceProfileRepository.Update(priceProfile);
            await _appDbContext.SaveChangesAsync();

            var priceProfileDto = _mapper.Map<PriceProfileDto>(priceProfile);

            await TryPublishCacheUpdated();

            return new OkObjectResult(priceProfileDto);
        }

        [HttpGet]
        [Route("")]
        public ObjectResult FindSingle(Guid id)
        {
            var priceProfile = _priceProfileRepository.FindSingleOrDefault(id);
            if (priceProfile == null)
            {
                return new OkObjectResult(null);
            }
            var priceProfileDto = _mapper.Map<PriceProfileDto>(priceProfile);
            return new OkObjectResult(priceProfileDto);
        }

        [HttpGet]
        [Route("resourceid")]
        public async Task<ObjectResult> FindSingleFromResourceId(Guid resourceId, string ppName = "standard")
        {
            var priceProfile = await _priceProfileRepository.FindSingleOrDefaultFromResourceId(resourceId, ppName);
            if (priceProfile == null)
            {
                return new OkObjectResult(null);
            }
            var priceProfileDto = _mapper.Map<PriceProfileDto>(priceProfile);
            return new OkObjectResult(priceProfileDto);
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindPriceProfiles(bool includeDeleted)
        {
            var priceProfiles = (await (includeDeleted ? _priceProfileRepository.FindAllIncludeDeleted() : _priceProfileRepository.FindAll())).ToList();
            var priceProfileDtos = priceProfiles.Select(x => _mapper.Map<PriceProfileDto>(x));
            return new OkObjectResult(priceProfileDtos);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("list/all/tenant")]
        public async Task<ObjectResult> FindPriceProfilesFromTenantId(Guid tenantId, bool includeDeleted = false)
        {
            var priceProfiles = (await _priceProfileRepository.FindAllFromTenantId(tenantId, includeDeleted)).ToList();
            var priceProfileDtos = priceProfiles.Select(x => _mapper.Map<PriceProfileDto>(x));
            return new OkObjectResult(priceProfileDtos);
        }

        [HttpPost]
        [Route("convert/all/tofixedprice/tenant")]
        public async Task<ObjectResult> ConvertAllToFixedPriceTenant(Guid tenantId)
        {
            // TODO: Replace Service Fabric client instantiation with proper dependency injection
            // var priceProfileClient = new PriceProfileClient(null);
            // await priceProfileClient.AuthorizeClient(tenantId);
            // var result = await priceProfileClient.ConvertToFixed();
            var result = "ConvertToFixed operation disabled - Service Fabric client needs replacement";
            return new ObjectResult(result);
        }

        [HttpPost]
        [Route("convert/all/tofixedprice")]
        public async Task<ObjectResult> ConvertAllToFixedPrice()
        {
            var articles = await _articleClient.FindAll(false);
            int imported = 0;
            foreach (var article in articles)
            {
                var priceProfiles = await _priceProfileRepository.FindFromResourceId(article.Id);
                if (priceProfiles.Any(x => !x.IsDeleted && x.FixedPrice != null && x.Name == "standard"))
                {
                    continue;
                }

                await ConvertToFixedPrice(article.Id);

                imported++;
            }
            await TryPublishCacheUpdated();
            return StatusCode(200, new ImportResultDto { CreatedCount = imported });
        }

        [HttpPost]
        [Route("convert/tofixedprice")]
        public async Task<ObjectResult> ConvertToFixedPrice(Guid resourceId)
        {
            var priceProfiles = await _priceProfileRepository.FindFromResourceId(resourceId);
            if (priceProfiles.Any(x => !x.IsDeleted && x.FixedPrice != null && x.Name == "standard"))
            {
                var invalidProfiles = priceProfiles.Where(x => !x.IsDeleted && x.FixedPrice == null && x.Name == "standard");
                foreach (var profile in invalidProfiles)
                {
                    profile.IsDeleted = true;
                }
                await _appDbContext.SaveChangesAsync();
                return new ObjectResult(null);
            }

            decimal? price = 999.99m;
            if (priceProfiles.Count == 0)
            {
                var newPricePriceProfile = _priceProfileFactory.CreatePricePriceProfile(resourceId, "standard", price);
                _priceProfileRepository.Add(newPricePriceProfile);

            }


            foreach (var priceProfile in priceProfiles)
            {
                price = await _priceCalculationClient.CalculatePrice(priceProfile.Id, DateTime.Now, DateTime.Now, GetTenantId());
                if (price == null)
                {
                    price = 999.99m;
                }

                var newPricePriceProfile = _priceProfileFactory.CreatePricePriceProfile(resourceId, priceProfile.Name, price);
                _priceProfileRepository.Add(newPricePriceProfile);

                priceProfile.IsDeleted = true;
                _priceProfileRepository.Update(priceProfile);
            }

            await _appDbContext.SaveChangesAsync();

            //var priceProfileDto = _mapper.Map<PriceProfileDto>(newPricePriceProfile);

            await TryPublishCacheUpdated();

            return new OkObjectResult(null);
        }

        private async Task TryPublishCacheUpdated()
        {
            try
            {
                // TODO: Replace Service Fabric MemCacheActor functionality
                // await _memCacheActor.PublishCacheUpdated(_claimsProvider.GetTenantId(), AppConstants.CACHE_COST_UPDATED);
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackException(exception);
            }
        }

        [HttpPut]
        [Route("modify/fixedprice")]
        public async Task<ObjectResult> ModifyFixedPrice(Guid priceProfileId, decimal fixedPrice)
        {
            var priceProfile = _priceProfileRepository.FindSingle(priceProfileId);
            if (priceProfile.FixedPrice != fixedPrice)
            {
                priceProfile.FixedPrice = fixedPrice;
                priceProfile.FixedPriceLastModified = DateTime.Now.ToEuTimeZone();
            }

            _priceProfileRepository.Update(priceProfile);
            await _appDbContext.SaveChangesAsync();

            var priceProfileDto = _mapper.Map<PriceProfileDto>(priceProfile);

            await TryPublishCacheUpdated();

            return new OkObjectResult(priceProfileDto);
        }

        [HttpPut]
        [Route("markdeleted")]
        public async Task<IActionResult> MarkPriceProfileAsDeleted([NoEmpty] Guid id)
        {
            var rentalUnit = _priceProfileRepository.FindSingleOrDefault(id);
            if (rentalUnit == null) return new NotFoundObjectResult(null);

            _priceProfileRepository.MarkAsDeleted(id);
            await _appDbContext.SaveChangesAsync();

            await TryPublishCacheUpdated();

            return new OkResult();
        }

        [HttpDelete]
        [Route("")]
        [Authorize(Roles = "Admin")]
        public async Task<ObjectResult> DeletePriceProfilePermanently([NoEmpty] Guid id)
        {
            var priceProfile = _priceProfileRepository.FindSingleOrDefault(id);
            if (priceProfile == null) return new NotFoundObjectResult(null);

            _priceProfileRepository.Delete(id);
            await _appDbContext.SaveChangesAsync();

            await TryPublishCacheUpdated();

            return new OkObjectResult(priceProfile);
        }

        [HttpDelete]
        [Route("resource")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePriceProfilePermanentlyFromResourceId([NoEmpty] Guid resourceId)
        {
            _priceProfileRepository.DeleteForResourceId(resourceId);
            await _appDbContext.SaveChangesAsync();

            await TryPublishCacheUpdated();

            return new OkResult();
        }
    }
}