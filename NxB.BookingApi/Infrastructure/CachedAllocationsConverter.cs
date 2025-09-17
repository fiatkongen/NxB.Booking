using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class RentalUnitsToCachedAllocationsConverter : ICachedAllocationsConverter
    {
        public async Task<List<CacheAllocation>> FromAllocations(List<Allocation> allocations)
        {
            var cacheAllocations = allocations.ToCacheAllocations().ToList();
            return cacheAllocations;
        }
    }

    public class RentalCategoriesToCachedAllocationsConverter : ICachedAllocationsConverter
    {
        private readonly IRentalUnitRepository _rentalUnitRepository;

        public RentalCategoriesToCachedAllocationsConverter(IRentalUnitRepository rentalUnitRepository)
        {
            this._rentalUnitRepository = rentalUnitRepository;
        }

        public async Task<List<CacheAllocation>> FromAllocations(List<Allocation> allocationsList)
        {
            var allocationIds = allocationsList.Select(x => x.RentalUnitId);
            var rentalUnits = await _rentalUnitRepository.FindAllFromAllocationIds(allocationIds);

            var cacheAllocations = allocationsList.Select(a =>
            {
                var rentalUnit = rentalUnits.Single(x => x.Id == a.RentalUnitId);
                return new CacheAllocation(rentalUnit.RentalCategoryId.ToString(), a.Start, a.End, a.Number);
            }).ToList();
            return cacheAllocations;
        }
    }
}