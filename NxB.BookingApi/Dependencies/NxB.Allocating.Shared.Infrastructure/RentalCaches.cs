using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NxB.Allocating.Shared.Model;

namespace NxB.Allocating.Shared.Infrastructure
{
    public class RentalCaches : IRentalCaches
    {
        private readonly IRentalCacheProvider _rentalCacheProvider;
        private readonly IRentalUnitRepository _rentalUnitRepository;
        private readonly ISmallRentalUnitCategoryRepository _smallRentalUnitCategoryRepository;
        private readonly ICachedAllocationsConverter _rentalUnitsCachedAllocationsConverter;

        public RentalCaches(IRentalCacheProvider rentalCacheProvider, IRentalUnitRepository rentalUnitRepository, ISmallRentalUnitCategoryRepository smallRentalUnitCategoryRepository )
        {
            _rentalCacheProvider = rentalCacheProvider;
            _rentalUnitRepository = rentalUnitRepository;
            _smallRentalUnitCategoryRepository = smallRentalUnitCategoryRepository;
            _rentalUnitsCachedAllocationsConverter = new RentalUnitsToCachedAllocationsConverter();

            // _rentalCategoriesCachedAllocationsConverter = new RentalCategoriesToCachedAllocationsConverter(rentalUnitRepository);
        }

        public async Task Initialize(DateTime start, DateTime end)
        {
            await _rentalCacheProvider.RentalUnitsCache.Initialize(start, end);
            await _rentalCacheProvider.RentalCategoriesCache.Initialize(start, end);
        }

        public async Task<bool> IsReadyForQuerying(DateTime start, DateTime end)
        {
            return (await _rentalCacheProvider.RentalUnitsCache.IsReadyForQuerying(start, end)) &&
                   (await _rentalCacheProvider.RentalCategoriesCache.IsReadyForQuerying(start, end));
        }

        public async Task AddAllocations(IEnumerable<Allocation> allocations)
        {
            var cacheAllocations = allocations.ToCacheAllocations().ToList();
            await _rentalCacheProvider.RentalCategoriesCache.AddAllocations(cacheAllocations);
            await _rentalCacheProvider.RentalUnitsCache.AddAllocations(cacheAllocations);
        }

        public async Task AddAllocationsAndSave(IEnumerable<Allocation> allocations)
        {
            var allocationsList = allocations.ToList();

            var cacheAllocations = await _rentalUnitsCachedAllocationsConverter.FromAllocations(allocationsList);
            await _rentalCacheProvider.RentalUnitsCache.AddAllocationsAndSave(cacheAllocations);
        }

        public async Task<Dictionary<string, decimal>> GetRentalCategoryAvailabilityAsCount(DateTime start, DateTime end)
        {
            return await this.GetRentalCategoryAvailabilityAsCount(start, end, await _smallRentalUnitCategoryRepository.Find());
        }

        public async Task<Dictionary<string, decimal>> GetRentalCategoryOnlineAvailabilityAsCount(DateTime start, DateTime end, Guid? filterRentalCategoryId)
        {
            var smallRentalUnitCategories = await _smallRentalUnitCategoryRepository.FindOnline(filterRentalCategoryId);
            return await this.GetRentalCategoryAvailabilityAsCount(start, end, smallRentalUnitCategories);
        }

        public async Task<Dictionary<string, decimal>> GetRentalCategoryKioskAvailabilityAsCount(DateTime start, DateTime end, Guid? filterRentalCategoryId)
        {
            return await this.GetRentalCategoryAvailabilityAsCount(start, end, await _smallRentalUnitCategoryRepository.FindKiosk(filterRentalCategoryId));
        }

        public async Task<Dictionary<string, decimal>> GetRentalCategoryAvailabilityAsCount(DateTime start, DateTime end, List<SmallRentalUnitCategory> smallRentalUnitCategories)
        {
            start = start.Date;
            end = end.Date;

            var rentalUnitAvailability = await _rentalCacheProvider.RentalUnitsCache.GetAvailabilityAsCount(start, end);
            var rentalCategoriesRentalUnitsCount =  smallRentalUnitCategories.ToDictionary(x => x.RentalCategoryId.ToString(), x => x.Count);

            foreach (var smallRentalUnitCategory in smallRentalUnitCategories)
            {
                foreach (var rentalUnitId in smallRentalUnitCategory.RentalUnitIds)
                {
                    var key = rentalUnitId.ToString();
                    if (rentalUnitAvailability.ContainsKey(key) && rentalUnitAvailability[key] == 0)
                    {
                        rentalCategoriesRentalUnitsCount[smallRentalUnitCategory.RentalCategoryId.ToString()]--;
                    }
                }
            }

            return rentalCategoriesRentalUnitsCount;
        }


        public async Task<Dictionary<string, decimal[]>> GetRentalUnitAvailabilityAsArrays(DateTime start, DateTime end)
        {
            start = start.Date;
            end = end.Date;

            return await this._rentalCacheProvider.RentalUnitsCache.GetAvailabilityAsArrays(start, end);
        }

        public async Task<Dictionary<string, decimal>> GetRentalUnitAvailabilityAsCount(DateTime start, DateTime end)
        {
            start = start.Date;
            end = end.Date;

            var availabilityCount = await _rentalCacheProvider.RentalUnitsCache.GetAvailabilityAsCount(start, end);
            return availabilityCount;
        }

        private async Task<Dictionary<string, decimal>> GetRentalUnitOnlineAvailabilityAsCount(DateTime start, DateTime end, List<SmallRentalUnitCategory> smallRentalUnitCategories)
        {
            List<string> onlineRentalUnitIds = smallRentalUnitCategories.Where(x => x.Count > 0).SelectMany(x => x.RentalUnitIds).Select(x => x.ToString()).ToList();
            var rentalUnitsOnlineAvailabilityAsCount = await _rentalCacheProvider.RentalUnitsCache.GetAvailabilityAsCount(start, end);
            rentalUnitsOnlineAvailabilityAsCount = rentalUnitsOnlineAvailabilityAsCount.Where(x => onlineRentalUnitIds.Contains(x.Key) && x.Value > 0).ToDictionary(x => x.Key, x => x.Value);
            return rentalUnitsOnlineAvailabilityAsCount;
        }

        public async Task<Dictionary<string, decimal>> GetRentalUnitOnlineAvailabilityAsCount(DateTime start, DateTime end, Guid? filterRentalCategoryId = null)
        {
            List<SmallRentalUnitCategory> smallRentalUnitCategories = await _smallRentalUnitCategoryRepository.FindOnline(filterRentalCategoryId);
            return await GetRentalUnitOnlineAvailabilityAsCount(start, end, smallRentalUnitCategories);
        }

        private async Task<Dictionary<string, decimal[]>> GetRentalUnitsFilteredAvailabilityAsArrays(DateTime start, DateTime end, List<SmallRentalUnitCategory> smallRentalUnitCategories)
        {
            start = start.Date;
            end = end.Date;
            List<string> onlineRentalUnitIds = smallRentalUnitCategories.Where(x => x.Count > 0).SelectMany(x => x.RentalUnitIds).Select(x => x.ToString()).ToList();

            var getRentalUnitOnlineAvailabilityAsArrays = await _rentalCacheProvider.RentalUnitsCache.GetAvailabilityAsArrays(start, end);

            getRentalUnitOnlineAvailabilityAsArrays = getRentalUnitOnlineAvailabilityAsArrays.Where(x => onlineRentalUnitIds.Contains(x.Key) && x.Value.Sum() > 0).ToDictionary(x => x.Key, x => x.Value);

            return getRentalUnitOnlineAvailabilityAsArrays;
        }


        public async Task<Dictionary<string, decimal[]>> GetRentalUnitOnlineAvailabilityAsArrays(DateTime start, DateTime end, Guid? filterRentalCategoryId = null)
        {
            List<SmallRentalUnitCategory> smallRentalUnitCategories = await _smallRentalUnitCategoryRepository.FindOnline(filterRentalCategoryId);
            return await GetRentalUnitsFilteredAvailabilityAsArrays(start, end, smallRentalUnitCategories);
        }

        public async Task<Dictionary<string, decimal>> GetRentalUnitKioskAvailabilityAsCount(DateTime start, DateTime end, Guid? filterRentalCategoryId = null)
        {
            List<SmallRentalUnitCategory> smallRentalUnitCategories = await _smallRentalUnitCategoryRepository.FindKiosk(filterRentalCategoryId);
            return await GetRentalUnitOnlineAvailabilityAsCount(start, end, smallRentalUnitCategories);
        }

        public async Task<Dictionary<string, decimal[]>> GetRentalUnitKioskAvailabilityAsArrays(DateTime start, DateTime end, Guid? filterRentalCategoryId = null)
        {
            List<SmallRentalUnitCategory> smallRentalUnitCategories = await _smallRentalUnitCategoryRepository.FindKiosk(filterRentalCategoryId);
            return await GetRentalUnitsFilteredAvailabilityAsArrays(start, end, smallRentalUnitCategories);
        }

        public async Task<Dictionary<string, decimal>> GetRentalUnitCtoutvertAvailabilityAsCount(DateTime start, DateTime end, Guid? filterRentalCategoryId = null)
        {
            List<SmallRentalUnitCategory> smallRentalUnitCategories = await _smallRentalUnitCategoryRepository.FindCtoutvert(filterRentalCategoryId);
            return await GetRentalUnitOnlineAvailabilityAsCount(start, end, smallRentalUnitCategories);
        }

        public async Task<Dictionary<string, decimal[]>> GetRentalUnitCtoutvertAvailabilityAsArrays(DateTime start, DateTime end, Guid? filterRentalCategoryId = null)
        {
            List<SmallRentalUnitCategory> smallRentalUnitCategories = await _smallRentalUnitCategoryRepository.FindCtoutvert(filterRentalCategoryId);
            return await GetRentalUnitsFilteredAvailabilityAsArrays(start, end, smallRentalUnitCategories);
        }

        public async Task SeedUnseededPartOfCache(DateTime start, DateTime end)
        {
            await this._rentalCacheProvider.RentalUnitsCache.SeedUnseededPartOfCache(start, end);
            await this._rentalCacheProvider.RentalCategoriesCache.SeedUnseededPartOfCache(start, end);
        }

        public async Task SaveChanges()
        {
            await this._rentalCacheProvider.RentalUnitsCache.SaveChanges();
            await this._rentalCacheProvider.RentalCategoriesCache.SaveChanges();
        }

        public void DeleteCache()
        {
            this._rentalCacheProvider.RentalUnitsCache.DeleteCache();
            this._rentalCacheProvider.RentalCategoriesCache.DeleteCache();
        }
    }
}
