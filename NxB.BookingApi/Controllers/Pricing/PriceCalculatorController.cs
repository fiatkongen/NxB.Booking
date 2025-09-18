using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.Clients;
using NxB.BookingApi.Models;
using NxB.Dto.TenantApi;
// TODO: Replace Service Fabric MemCacheActor functionality
// using NxB.MemCacheActor.Interfaces;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Infrastructure;
using NxB.Settings.Shared.Infrastructure;

namespace NxB.BookingApi.Controllers.Pricing
{
    [Produces("application/json")]
    [Route("pricecalculation")]
    [Authorize]
    [ApiValidationFilter]
    public class PriceCalculatorController : BaseController
    {
        private readonly IPriceCalculator _priceCalculator;
        private readonly ITenantClient _tenantClient;
        private readonly IPriceProfileRepository _priceProfileRepository;
        private readonly ICostIntervalRepository _costIntervalRepository;
        private readonly TelemetryClient _telemetryClient;
        private readonly IClaimsProvider _claimsProvider;
        private readonly ISettingsRepository _settingsRepository;
        private readonly AppDbContext _appDbContext;

        public PriceCalculatorController(ITenantClient tenantClient, IPriceProfileRepository priceProfileRepository, ICostIntervalRepository costIntervalRepository, TelemetryClient telemetryClient, IClaimsProvider claimsProvider, IPriceCalculator priceCalculator, ISettingsRepository settingsRepository, AppDbContext appDbContext)
        {
            _tenantClient = tenantClient;
            _priceProfileRepository = priceProfileRepository;
            _costIntervalRepository = costIntervalRepository;
            _telemetryClient = telemetryClient;
            _claimsProvider = claimsProvider;
            _priceCalculator = priceCalculator;
            _settingsRepository = settingsRepository;
            _appDbContext = appDbContext;
        }

        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public async Task<ObjectResult> CalculatePrice(Guid priceProfileId, DateTime start, DateTime end, string name = "", Guid? tenantId = null, decimal? currentPrice = null, DateTime? currentPriceCreateDate = null)
        {
            _appDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            try
            {
                var costInformation = await _priceCalculator.CalculatePrice(priceProfileId, start, end,
                    tenantId ?? _claimsProvider.GetTenantId(), _priceProfileRepository, _costIntervalRepository, false, currentPrice, currentPriceCreateDate);
                var price = costInformation?.Cost;
                LogNewPriceCalculationSuccess(true);
                if (costInformation != null && costInformation.IsCacheHit)
                {
                    this.LogPriceCalculationCacheHit();
                }

                return price.HasValue ? new OkObjectResult(price) : new OkObjectResult(double.NaN);
            }
            catch (CostCalculationException exception)
            {
                LogNewPriceCalculationSuccess(false);
                return new OkObjectResult(double.NaN);
            }
            catch (PriceCalculationException priceCalculationException)
            {
                LogNewPriceCalculationSuccess(false);
                return BadRequest(priceCalculationException.Message);
            }
            catch
            {
                LogNewPriceCalculationSuccess(false);
                throw;
            }
        }

        [HttpGet]
        [Route("costinformation")]
        public async Task<ObjectResult> CalculateCostInformation(Guid priceProfileId, DateTime start, DateTime end, string name = "", Guid? tenantId = null)
        {
            _appDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            try
            {
                var costInformation = await _priceCalculator.CalculatePrice(priceProfileId, start, end, tenantId ?? _claimsProvider.GetTenantId(), _priceProfileRepository, _costIntervalRepository, true);

                return new OkObjectResult(costInformation);
            }
            catch (CostCalculationException priceCalculationException)
            {
                return BadRequest(priceCalculationException.Message);
            }
            catch (PriceCalculationException priceCalculationException)
            {
                return BadRequest(priceCalculationException.Message);
            }
        }

        private void LogPriceCalculationCacheHit()
        {
            var metrics = new Dictionary<string, double>
            {
                {"PriceCalculation Cache hit", 1},
            };

            //_telemetryClient.TrackEvent("PricingMetrics", metrics: metrics); //too much logging
        }

        private void LogNewPriceCalculationSuccess(bool success)
        {
            var metrics = new Dictionary<string, double>
            {
                {"PriceCalculation(new) Failure", success ? 0 : 1},
                {"PriceCalculation(new) Success", success ? 1 : 0},
            };

            //_telemetryClient.TrackEvent("PricingMetrics", metrics: metrics); //too much logging
        }
    }
}