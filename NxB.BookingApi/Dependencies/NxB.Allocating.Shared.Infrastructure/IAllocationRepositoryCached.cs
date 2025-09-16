using System;
using System.Threading.Tasks;
using NxB.Allocating.Shared.Model;

namespace NxB.Allocating.Shared.Infrastructure
{
    public interface IAllocationRepositoryCached : IAllocationRepository
    {
        Task PreFectAllocations(DateTime start, DateTime end);

    }
}