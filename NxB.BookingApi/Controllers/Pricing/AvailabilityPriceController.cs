using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
// TODO: Remove or replace Allocating.Shared.Model references that don't exist
// using NxB.Allocating.Shared.Model;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.AllocationApi;
using NxB.Dto.Clients;
using NxB.Dto.PricingApi;
using NxB.BookingApi.Models;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Infrastructure;
using NxB.Settings.Shared.Infrastructure;

namespace NxB.BookingApi.Controllers.Pricing
{
    [Produces("application/json")]
    [Route("availabilityprice")]
    [Authorize]
    [ApiValidationFilter]
    public class AvailabilityPriceController : BaseController
    {
        private readonly IPriceProfileRepository _priceProfileRepository;
        private readonly ICostIntervalRepository _costIntervalRepository;
        private readonly TelemetryClient _telemetryClient;
        private readonly ITimeSpanService<TimeSpanBase> _baseTimeSpanService;
        private readonly IAvailabilityClient _availabilityClient;
        private readonly ITimeSpanService<TimeSpanBase> _timeSpanBaseService;

        public AvailabilityPriceController(IPriceProfileRepository priceProfileRepository, ICostIntervalRepository costIntervalRepository, TelemetryClient telemetryClient, ITimeSpanService<TimeSpanBase> baseTimeSpanService, IAvailabilityClient availabilityClient, ITimeSpanService<TimeSpanBase> timeSpanBaseService)
        {
            _priceProfileRepository = priceProfileRepository;
            _costIntervalRepository = costIntervalRepository;
            _telemetryClient = telemetryClient;
            _baseTimeSpanService = baseTimeSpanService;
            _availabilityClient = availabilityClient;
            _timeSpanBaseService = timeSpanBaseService;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("arrays/tenant")]
        public async Task<ObjectResult> BuildOnlineAvailabilityArraysForTenant(DateTime start, DateTime end, string type, Guid tenantId, Guid? filterRentalCategoryId = null)
        {
            var sw = Stopwatch.StartNew();
            // TODO: Replace Service Fabric client instantiation with proper dependency injection
            // var availabilityClient = new AvailabilityClient(null);
            // await availabilityClient.AuthorizeClient(tenantId);

            // var rentalCategoryClient = new RentalCategoryClient(null);
            // await rentalCategoryClient.AuthorizeClient(tenantId);

            var priceProfiles = await _priceProfileRepository.FindAllFromTenantId(tenantId, true);
            var timeSpans = await _timeSpanBaseService.FindAllWithinForTenant(tenantId, new DateInterval(start, end));
            var costIntervals = await _costIntervalRepository.FindAllFromTenantId(tenantId);
            // TODO: Replace Service Fabric client calls with proper dependency injection
            // var rentalCategories = await rentalCategoryClient.FindAllFromType(type);
            var rentalCategories = new List<dynamic>(); // Temporary placeholder
            if (filterRentalCategoryId != null)
            {
                rentalCategories = rentalCategories.Where(x => x.Id == filterRentalCategoryId.Value).ToList();
            }
            List<AvailabilityPriceDto> allAvailabilityPriceDtos = new List<AvailabilityPriceDto>();

            // TODO: Replace this loop when Service Fabric clients are properly implemented
            // foreach (var rentalCategory in rentalCategories)
            // {
            //     var availabilityPriceBuilder = new AvailabilityPriceBuilder(_availabilityClient, _priceProfileRepository, _costIntervalRepository, _telemetryClient, _baseTimeSpanService);
            //     var availabilityPriceDtos = await availabilityPriceBuilder.Build(tenantId, start.Highest(DateTime.Now.Date), end, type, rentalCategory.Id, priceProfiles, costIntervals, timeSpans);
            //     allAvailabilityPriceDtos = allAvailabilityPriceDtos.Concat(availabilityPriceDtos).ToList();
            // }

            Debug.WriteLine("BuildOnlineAvailabilityArraysForTenant in " + sw.Elapsed.TotalSeconds + " seconds");

            return new ObjectResult(allAvailabilityPriceDtos);
        }

        [HttpGet]
        [Route("arrays")]
        public async Task<ObjectResult> BuildOnlineAvailabilityArrays(DateTime start, DateTime end, Guid rentalCategoryId, string type)
        {
            var sw = Stopwatch.StartNew();

            var tenantId = this.GetTenantId();

            _telemetryClient.TrackTrace("AvailabilityPriceController.AvailabilityPriceBuilder: start for " + tenantId);

            var availabilityPriceBuilder = new AvailabilityPriceBuilder(_availabilityClient, _priceProfileRepository, _costIntervalRepository, _telemetryClient, _baseTimeSpanService);

            var availabilityPriceDtos = await availabilityPriceBuilder.Build(tenantId, start, end, type, rentalCategoryId);

            var availabilityPriceGenerationSeconds = sw.Elapsed.TotalSeconds;
            LogAvailabilityPriceMetric(availabilityPriceGenerationSeconds, availabilityPriceDtos.Count);
            Debug.WriteLine($"BuildOnlineAvailabilityArrays in {availabilityPriceGenerationSeconds} seconds");
            _telemetryClient.TrackTrace($"AvailabilityPriceController.AvailabilityPriceBuilder: end in {availabilityPriceGenerationSeconds} seconds");

            return new ObjectResult(availabilityPriceDtos);
        }

        private void LogAvailabilityPriceMetric(double generationSeconds, int items)
        {
            var metrics = new Dictionary<string, double>
            {
                {"Ctoutvert Availability Build Time", generationSeconds},
                {"Ctoutvert Availability Items Count", items},
            };
            _telemetryClient.TrackEvent("PriceMetrics", metrics: metrics);
        }
    }
}