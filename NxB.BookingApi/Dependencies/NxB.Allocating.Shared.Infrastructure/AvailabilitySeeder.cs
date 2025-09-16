using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munk.Utils.Object;
using NxB.Allocating.Shared.Model;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;

namespace NxB.Allocating.Shared.Infrastructure
{
    public class AvailabilitySeeder : IAvailabilitySeeder
    {
        private readonly ICachedAllocationsConverter _cachedAllocationsConverter;
        private readonly IAllocationRepositoryCached _allocationRepositoryCached;

        public AvailabilitySeeder(IAllocationRepositoryCached allocationRepositoryCached, ICachedAllocationsConverter cachedAllocationsConverter)
        {
            _cachedAllocationsConverter = cachedAllocationsConverter;
            _allocationRepositoryCached = allocationRepositoryCached;
        }

        public async Task PreFetchAllocations(DateTime start, DateTime end)
        {
            await _allocationRepositoryCached.PreFectAllocations(start, end);
        }

        public virtual async Task SeedMatrix(AvailabilityMatrix availabilityMatrix)
        {
            var allocations = await _allocationRepositoryCached.FindWithinInterval(availabilityMatrix.Start, availabilityMatrix.End);
            var cacheAllocations = (await _cachedAllocationsConverter.FromAllocations(allocations)).ToArray();
            availabilityMatrix.AddAllocations(cacheAllocations);
            availabilityMatrix.IsSeeded = true;
        }
    }
}
