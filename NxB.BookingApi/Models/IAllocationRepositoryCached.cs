using System;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public interface IAllocationRepositoryCached : IAllocationRepository
    {
        Task PreFectAllocations(DateTime start, DateTime end);

    }
}