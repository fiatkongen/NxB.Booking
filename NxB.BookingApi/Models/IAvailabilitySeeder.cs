using System;
using System.Threading.Tasks;
using NxB.Allocating.Shared.Infrastructure;

namespace NxB.BookingApi.Models
{
    public interface IAvailabilitySeeder
    {
        Task PreFetchAllocations(DateTime start, DateTime end);
        Task SeedMatrix(AvailabilityMatrix availabilityMatrix);
    }
}