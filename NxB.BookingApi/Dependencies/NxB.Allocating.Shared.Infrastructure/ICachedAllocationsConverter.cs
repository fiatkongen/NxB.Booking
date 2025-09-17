using System.Collections.Generic;
using System.Threading.Tasks;
using NxB.BookingApi.Models;

namespace NxB.Allocating.Shared.Infrastructure
{
    public interface ICachedAllocationsConverter
    {
        Task<List<CacheAllocation>> FromAllocations(List<Allocation> allocationsList);
    }
}