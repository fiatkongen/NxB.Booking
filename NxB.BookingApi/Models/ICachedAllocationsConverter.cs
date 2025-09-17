using System.Collections.Generic;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public interface ICachedAllocationsConverter
    {
        Task<List<CacheAllocation>> FromAllocations(List<Allocation> allocationsList);
    }
}