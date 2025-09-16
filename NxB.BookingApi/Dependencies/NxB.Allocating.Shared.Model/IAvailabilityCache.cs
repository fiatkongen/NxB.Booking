using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;

namespace NxB.Allocating.Shared.Model
{
    public interface IAvailabilityCache : IAvailabilityCacheReader, IAvailabilityCacheWriter
    {}

    public interface IAvailabilityCacheInitializer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns>True if all parts of cache was already initialized</returns>
        Task Initialize(DateTime start, DateTime end);
        Task<bool> IsReadyForQuerying(DateTime start, DateTime end);
        Task SeedUnseededPartOfCache(DateTime start, DateTime end);
        Task SaveChanges();
        void DeleteCache();
    }

    public interface IAvailabilityCacheReader : IAvailabilityCacheInitializer
    {
        Task<Dictionary<string, decimal[]>> GetAvailabilityAsArrays(DateTime start, DateTime end);
        Task<Dictionary<string, decimal>> GetAvailabilityAsCount(DateTime start, DateTime end);
    }

    public interface IAvailabilityCacheWriter : IAvailabilityCacheInitializer
    {
        Task AddAllocations(IEnumerable<CacheAllocation> allocations);
        Task AddAllocationsAndSave(IEnumerable<CacheAllocation> allocations);
    }
}