using System;
using System.Threading.Tasks;

namespace NxB.Allocating.Shared.Infrastructure
{
    public interface IAvailabilitySeeder
    {
        Task PreFetchAllocations(DateTime start, DateTime end);
        Task SeedMatrix(AvailabilityMatrix availabilityMatrix);
    }
}
