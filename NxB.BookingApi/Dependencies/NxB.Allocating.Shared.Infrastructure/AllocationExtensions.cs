using System.Collections.Generic;
using System.Linq;
using NxB.Allocating.Shared.Model;

namespace NxB.Allocating.Shared.Infrastructure
{
    public static class AllocationExtensions
    {
        public static IEnumerable<CacheAllocation> ToCacheAllocations(this IEnumerable<Allocation> allocations)
        {
            return allocations.Select(x => new CacheAllocation(x));
        }
    }
}
