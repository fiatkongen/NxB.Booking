using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Itenso.TimePeriod;
using NxB.Allocating.Shared.Infrastructure;
using NxB.BookingApi.Models;
using NxB.BookingApi.Models.Exceptions;

namespace NxB.BookingApi.Infrastructure
{
    public class AvailabilityGuard
    {
        private readonly IAllocationRepository _allocationRepository;
        private readonly IRentalUnitRepository _rentalUnitRepository;

        public AvailabilityGuard(IAllocationRepository allocationRepository, IRentalUnitRepository rentalUnitRepository)
        {
            _allocationRepository = allocationRepository;
            _rentalUnitRepository = rentalUnitRepository;
        }

        public virtual async Task<bool> IsUnitAvailableForInterval(Guid unitId, DateInterval dateInterval, decimal minimumAvailability, IEnumerable<Allocation> extraAllocations = null)
        {
            AvailabilityArray availabilityArray = new AvailabilityArray(dateInterval.Start, dateInterval.End);

            if (extraAllocations != null)
            {
                var extraAllocationsWithinDateIntervalAndForSameUnit = extraAllocations.Where(x => x.RentalUnitId == unitId && x.DateInterval.TimeBlock.OverlapsWith(dateInterval.TimeBlock)).ToCacheAllocations().ToArray();
                availabilityArray.AddAllocations(extraAllocationsWithinDateIntervalAndForSameUnit);
            }

            var allocations = await _allocationRepository.FindWithinIntervalForUnit(dateInterval.Start, availabilityArray.End, unitId);
            if (allocations.Count == 0)
                throw new AvailabilityException($"No availablity were found for unit with id: {unitId}");

            availabilityArray.AddAllocations(allocations.ToCacheAllocations().ToArray());
            var availability = availabilityArray.GetAvailability(dateInterval.Start, dateInterval.End);

            if (availability - minimumAvailability > 1)
                throw new AvailabilityOverReleasedException(unitId, _rentalUnitRepository.TryGetRentalUnitName(unitId));

            return availability >= minimumAvailability;
        }

        public virtual async Task<Allocation> AreUnitsGloballyAvailableForAllocations(List<Allocation> allocations)
        {
            foreach (var allocation in allocations)
            {
                var result = await IsUnitAvailableForInterval(allocation.RentalUnitId,
                    allocation.DateInterval, 0 - allocation.Number, allocations.Except(new[] { allocation }).Select(x => x));
                if (!result) return allocation;
                _allocationRepository.Add(allocation);
            }

            return null;
        }

    }
}