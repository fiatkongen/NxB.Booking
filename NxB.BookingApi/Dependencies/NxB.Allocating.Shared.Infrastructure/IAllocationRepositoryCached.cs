using System;
using System.Threading.Tasks;
using NxB.BookingApi.Models;

namespace NxB.Allocating.Shared.Infrastructure
{
    public interface IAllocationRepositoryCached : IAllocationRepository
    {
        Task PreFectAllocations(DateTime start, DateTime end);

    }
}