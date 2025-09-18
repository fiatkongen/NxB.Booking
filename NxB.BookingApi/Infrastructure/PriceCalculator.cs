using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Newtonsoft.Json;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class PriceCalculator : IPriceCalculator
    {
        public int TotalCalculations = 0;
        private List<PriceProfile> _cachedPriceProfiles = new();
        private List<CostInterval> _cachedIntervals = new();
        private readonly ConcurrentDictionary<string, CostInformation> _cachedPrices = new();
        private readonly TelemetryClient _telemetryClient;

        public PriceCalculator(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        public async Task<(List<PriceProfile>, List<CostInterval>)> CacheAll(Guid tenantId, IPriceProfileRepository priceProfileRepository, ICostIntervalRepository costIntervalRepository)
        {
            var priceProfiles = await priceProfileRepository.FindAllFromTenantId(tenantId, true);
            var costIntervals = await costIntervalRepository.FindAllFromTenantId(tenantId);

            lock (_cachedPriceProfiles)
            {
                this._cachedPriceProfiles = priceProfiles;
            }

            lock (_cachedIntervals)
            {
                this._cachedIntervals = costIntervals;
            }
            return (priceProfiles, costIntervals);
        }

        public async Task<CostInformation> CalculatePrice(Guid priceProfileId, DateTime start, DateTime end, Guid tenantId, IPriceProfileRepository priceProfileRepository, ICostIntervalRepository costIntervalRepository, bool ignoreCache = false, decimal? currentPrice = null, DateTime? currentPriceCreateDate = null)
        {
            PriceProfile cachedPriceProfile;

            start = start.Date;
            end = end.Date;


            if (currentPriceCreateDate.HasValue && currentPrice.HasValue)
            {
                lock (_cachedPriceProfiles)
                {
                    cachedPriceProfile = GetAndCachePriceProfile(priceProfileId, priceProfileRepository, tenantId);
                    if (cachedPriceProfile.FixedPrice != null && cachedPriceProfile.FixedPriceLastModified != null &&
                        currentPriceCreateDate.Value < cachedPriceProfile.FixedPriceLastModified.Value)
                    {
                        return new CostInformation(start, end, 1, "Fast pris", currentPrice);
                    }
                }
            }

            var cachedPriceKey = tenantId + "_" + priceProfileId + "_" + start.ToJsonDateString() + "_" + end.ToJsonDateString();
            if (!ignoreCache && _cachedPrices.TryGetValue(cachedPriceKey, out var cachedPrice))
            {
                return cachedPrice;
            }

            cachedPriceProfile = GetAndCachePriceProfile(priceProfileId, priceProfileRepository, tenantId);

            if (cachedPriceProfile.FixedPrice != null)
                return new CostInformation(start, end, 1, "Fast pris", cachedPriceProfile.FixedPrice);

            List<CostInterval> costIntervals = null;
            lock (_cachedIntervals)
            {
                costIntervals = _cachedIntervals.Where(x => x.TenantId == tenantId).ToList();
            }

            if (costIntervals.Count == 0)
            {
                var allCostIntervals = await costIntervalRepository.FindAllFromTenantId(tenantId);
                lock (_cachedIntervals)
                {
                    _cachedIntervals.AddRange(allCostIntervals);
                }
            }

            lock (_cachedIntervals)
            {
                costIntervals = _cachedIntervals.Where(x => x.TenantId == tenantId && x.PriceProfileId == priceProfileId).ToList();
            }

            if (costIntervals.Count == 0)
            {
                throw new NoCostCalculationsImportedException(start, end);
            }

            var costCalculator = new CostCalculator(costIntervals);
            var costInformation = costCalculator.CalculateCost(start, end, false, null, new CostCalculationContext(start, end));
            TotalCalculations += costCalculator.TotalCalculations;
            if (costInformation?.Cost != null)
            {
                _cachedPrices.TryAdd(cachedPriceKey, costInformation);
                costInformation.IsCacheHit = true;
                return costInformation;
            }

            throw new CostCalculationException(start, end);
        }

        private PriceProfile GetAndCachePriceProfile(Guid priceProfileId, IPriceProfileRepository priceProfileRepository, Guid tenantId)
        {
            PriceProfile cachedPriceProfile;

            lock (_cachedPriceProfiles)
            {
                cachedPriceProfile = _cachedPriceProfiles.FirstOrDefault(x => x.Id == priceProfileId);
            }

            if (cachedPriceProfile == null)
            {
                cachedPriceProfile = priceProfileRepository.FindSingleOrDefault(priceProfileId);
                if (cachedPriceProfile == null)
                {
                    var message = $"CalculatePrice: Kunne ikke finde prisprofil med id: {priceProfileId} for tenant {tenantId}";
                    throw new PriceCalculationException(message);
                }

                lock (_cachedPriceProfiles)
                {
                    _cachedPriceProfiles.Add(cachedPriceProfile);
                }
            }
            return cachedPriceProfile;
        }

        public void ClearCaches(Guid tenantId)
        {
            _telemetryClient.TrackTrace("PriceCalculator.ClearCaches for " + tenantId);
            lock (_cachedPriceProfiles)
            {
                _cachedPriceProfiles.RemoveAll(x => x.TenantId == tenantId);
            }

            lock (_cachedIntervals)
            {
                _cachedIntervals.RemoveAll(x => x.TenantId == tenantId);
            }

            foreach (var key in _cachedPrices.Keys)
            {
                if (key.Contains(tenantId.ToString()))
                {
                    _cachedPrices.TryRemove(key, out var value);
                }
            }
        }
    }
}