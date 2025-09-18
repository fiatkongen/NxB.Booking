using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using NxB.Allocating.Shared.Infrastructure;
// TODO: Remove or replace Allocating.Shared.Model references that don't exist
// using NxB.Allocating.Shared.Model;
using NxB.Dto.AllocationApi;
using NxB.Dto.Clients;
using NxB.Dto.PricingApi;
// TODO: Remove old namespace references
// using NxB.PricingApi.Exceptions;
using NxB.BookingApi.Exceptions;
using ServiceStack.Text;


namespace NxB.BookingApi.Models
{
    public class AvailabilityPriceBuilder
    {
        private List<PriceProfile> _cachedPriceProfiles = new();
        private readonly Dictionary<string, AvailabilityPriceDto> _cachedAvailabilityPrices;

        private readonly IAvailabilityClient _availabilityClient;
        private readonly IPriceProfileRepository _priceProfileRepository;
        private readonly ICostIntervalRepository _costIntervalRepository;
        private readonly TelemetryClient _telemetryClient;
        private int _totalCalculations;
        private readonly ConcurrentDictionary<string, CostInformation> _optimizerCostTree;
        private readonly ITimeSpanService<TimeSpanBase> _timeSpanBaseService;

        public AvailabilityPriceBuilder(IAvailabilityClient availabilityClient,
            IPriceProfileRepository priceProfileRepository,
            ICostIntervalRepository costIntervalRepository,
            TelemetryClient telemetryClient,
            ITimeSpanService<TimeSpanBase> timeSpanBaseService)
        {
            _availabilityClient = availabilityClient;
            _priceProfileRepository = priceProfileRepository;
            _costIntervalRepository = costIntervalRepository;
            _telemetryClient = telemetryClient;
            _timeSpanBaseService = timeSpanBaseService;
            _cachedAvailabilityPrices = new Dictionary<string, AvailabilityPriceDto>();
            _optimizerCostTree = new ConcurrentDictionary<string, CostInformation>();
        }

        public async Task<List<AvailabilityPriceDto>> Build(Guid tenantId, DateTime start, DateTime end, string type, Guid rentalCategoryId)
        {
            var priceProfiles = await _priceProfileRepository.FindAllFromTenantId(tenantId, true);
            var costIntervals = await _costIntervalRepository.FindAllFromTenantId(tenantId);
            var timeSpanBases = await _timeSpanBaseService.FindAllWithinForTenant(tenantId, new DateInterval(start, end));

            return await this.Build(tenantId, start, end, type, rentalCategoryId, priceProfiles, costIntervals, timeSpanBases);
        }

        public async Task<List<AvailabilityPriceDto>> Build(Guid tenantId, DateTime start, DateTime end, string type, Guid rentalCategoryId, List<PriceProfile> priceProfiles, List<CostInterval> costIntervals, List<TimeSpanBase> timeSpanBases)
        {
            var rentalUnitsAvailability = await _availabilityClient.GetRentalUnitAvailabilityAsArrays(start, end, type, rentalCategoryId);
            _cachedPriceProfiles = priceProfiles;

            //var tasks = new List<Task>();
            foreach (var resourceId in rentalUnitsAvailability.Keys)
            {
                //tasks.Add(BuildForResource(tenantId, start, resourceId, rentalUnitsAvailability, costIntervals, timeSpanBases));
                await BuildForResource(tenantId, start, resourceId, rentalCategoryId, rentalUnitsAvailability, costIntervals, timeSpanBases);
            }
            //Task.Run()
            //await Task.WhenAll(tasks);

            var priceDtos = _cachedAvailabilityPrices.Values.ToList().Where(x => x != null).OrderBy(x => x.RentalCategoryId).ThenBy(x => x.StartDate).ThenBy(x => x.IncludedEndDate).ToList();
            Debug.WriteLine("priceCalculator count: " + _totalCalculations);
            Debug.WriteLine("availabilityPriceDtos items: " + priceDtos.Count);
            return priceDtos;
        }

        private async Task BuildForResource(Guid tenantId, DateTime start, string resourceId, Guid rentalCategoryId,
            Dictionary<string, decimal[]> rentalUnitsAvailability, List<CostInterval> costIntervals,
            List<TimeSpanBase> timeSpanBases)
        {
            var rentalCategoryDefaultOnlinePriceProfileId = await GetRentalCategoryDefaultOnlinePriceProfileId(rentalCategoryId);

            if (rentalCategoryDefaultOnlinePriceProfileId == null)
            {
                _telemetryClient.TrackTrace(
                    $"AvailabilityPrice. Could not find priceProfile for rentalCategory {rentalCategoryId} for tenant {tenantId}");
                return;
            }

            var availabilityDays = rentalUnitsAvailability[resourceId];
            int maxDurationDays = availabilityDays.Length.Lowest(28);

            var relevantCostIntervals = costIntervals
                .Where(x => x.PriceProfileId == rentalCategoryDefaultOnlinePriceProfileId.Value).ToList();
            var costCalculator = new CostCalculator(relevantCostIntervals);

            var resourceTimeSpans = timeSpanBases
                .OrderBy(x => x.Type)
                .Where(x => x.ResourceId == rentalCategoryId.ToString() || x.ResourceId == tenantId.ToString())
                .Distinct(new TimeSpanBaseComparer())
                .ToList();

            for (int additionalDurationDays = 0; additionalDurationDays < maxDurationDays; additionalDurationDays++)
            {
                for (int dayCount = 0; (dayCount + additionalDurationDays) < availabilityDays.Length; dayCount++)
                {
                    var relevantDays = availabilityDays.Skip(dayCount).Take(1 + additionalDurationDays).ToList();

                    bool isAvailable = relevantDays.All(x => x > 0);
                    if (!isAvailable)
                        continue;

                    var startDate = start.AddDays(dayCount);
                    var endDate = start.AddDays(dayCount + 1 + additionalDurationDays);
                    var includedEndDate = endDate.AddDays(-1);

                    var key = rentalCategoryId + "_" + startDate.ToFileTime() + "_" + endDate.ToFileTime();

                    if (_cachedAvailabilityPrices.ContainsKey(key))
                    {
                        // Debug.WriteLine("Price already calculated for " + rentalCategoryName);
                        continue;
                    }


                    var dateInterval = new DateInterval(startDate, endDate);
                    var isValidTimeSpan = true;
                    if (resourceTimeSpans.Count > 0)
                    {
                        isValidTimeSpan =
                            await _timeSpanBaseService.ValidateTimeSpan(dateInterval, resourceTimeSpans, tenantId);
                    }

                    if (!isValidTimeSpan)
                    {
                        _cachedAvailabilityPrices.TryAdd(key, null);
                        continue;
                    }

                    try
                    {
                        var costInformation = costCalculator.CalculateCost(startDate, endDate, true, this._optimizerCostTree, new CostCalculationContext(startDate, endDate));
                        _totalCalculations += costCalculator.TotalCalculations;

                        if (costInformation != null && costInformation.Cost.HasValue)
                        {
                            var availabilityPriceDto = new AvailabilityPriceDto
                            {
                                RentalCategoryId = rentalCategoryId,
                                StartDate = startDate,
                                IncludedEndDate = includedEndDate,
                                Price = costInformation.Cost.Value == 0
                                    ? 0
                                    : costInformation.Cost.Value //; / _euroConversionRate
                            };
                            _cachedAvailabilityPrices.TryAdd(key, availabilityPriceDto);
                        }
                    }
                    catch (Exception exception)
                    {
                        _telemetryClient.TrackException(
                            new AvailabilityPriceException($"AvailabilityPrice. Exception: {exception}"));
                    }
                }
            }
        }

        private async Task<Guid?> GetRentalCategoryDefaultOnlinePriceProfileId(Guid rentalCategoryId)
        {
            var priceProfilesForRentalCategory = _cachedPriceProfiles.Where(x => x.ResourceId == rentalCategoryId && !x.IsDeleted).ToList();

            var priceProfile = priceProfilesForRentalCategory.FirstOrDefault(x => x.Name == "online");

            //return ctoutvertPP?.Id;
            if (priceProfile != null)
            {
                return priceProfile.Id;
            }

            // var rentalCategoryDefaultOnlinePriceProfileId = rentalCategory.DefaultOnlinePriceProfileId ?? rentalCategory.DefaultPriceProfileId;

            return priceProfilesForRentalCategory
                .FirstOrDefault(x => !x.IsDeleted && x.Name == "standard")?.Id;
        }
    }

    // Hack for removing doublets of the timebases (to address the problem with  two eternal -1 allocations)
    public class TimeSpanBaseComparer : IEqualityComparer<TimeSpanBase>
    {
        public bool Equals(TimeSpanBase x, TimeSpanBase y)
        {
            if (x == null || y == null || (x.Start.Year != 2000 && x.End.Year != 2100))
                return false;

            return x.Start == y.Start && x.End == y.End && x.OpenClosed == y.OpenClosed && x.ParameterNumber == y.ParameterNumber;
        }

        public int GetHashCode(TimeSpanBase obj)
        {
            return HashCode.Combine(obj.Start, obj.End, obj.ParameterNumber, obj.OpenClosed);
        }
    }
}