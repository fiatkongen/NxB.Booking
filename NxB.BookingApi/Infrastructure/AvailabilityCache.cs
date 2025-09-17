using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Transactions;
using Itenso.TimePeriod;
using Microsoft.EntityFrameworkCore;
using NxB.BookingApi.Models;
using NxB.BookingApi.Models.Exceptions;

namespace NxB.BookingApi.Infrastructure
{
    public class AvailabilityCache : IAvailabilityCache
    {
        private readonly IAvailabilitySeeder _availabilitySeeder;
        private readonly IAvailabilityMatrixRepository _availabilityMatrixRepository;
        private readonly AvailabilityMatrixFactory _availabilityMatrixFactory;
        private readonly ITimeChunkDivider _timeChunkDivider;

        public AvailabilityCache(
            ITimeChunkDivider timeChunkDivider,
            IAvailabilitySeeder availabilitySeeder,
            IAvailabilityMatrixRepository availabilityMatrixRepository,
            AvailabilityMatrixFactory availabilityMatrixFactory
            )
        {
            _availabilitySeeder = availabilitySeeder;
            _availabilityMatrixRepository = availabilityMatrixRepository;
            _availabilityMatrixFactory = availabilityMatrixFactory;
            _timeChunkDivider = timeChunkDivider;
        }

        public async Task AddAllocations(IEnumerable<CacheAllocation> allocations)
        {
            var unitGroups = allocations.GroupBy(x => x.ResourceId, x => x, (key, a) => new { unitId = key.ToString(), cacheAllocations = a.ToList() }).ToList();

            foreach (var unitGroup in unitGroups)
            {
                var cacheAllocations = unitGroup.cacheAllocations;
                var minStart = cacheAllocations.Min(x => x.Start);
                var maxEnd = cacheAllocations.Max(x => x.End);
                var availabilityMatrices = await GetAvailabilityMatrices(minStart, maxEnd);
                VerifyMatricesAreSeeded(availabilityMatrices);
                availabilityMatrices.ForEach(x => x.AddAllocations(cacheAllocations.ToArray()));
            }
        }

        private void VerifyMatricesAreSeeded(List<AvailabilityMatrix> availabilityMatrices)
        {
            var unseededMatrix = availabilityMatrices.FirstOrDefault(x => !x.IsSeeded);
            if (unseededMatrix != null)
            {
                throw new AvailabilityCacheUnseeded(unseededMatrix.Start, unseededMatrix.End);
            }
        }

        public async Task AddAllocationsAndSave(IEnumerable<CacheAllocation> allocations)
        {
            await AddAllocations(allocations);
            await SaveChanges();
        }

        public async Task<Dictionary<string, decimal[]>> GetAvailabilityAsArrays(DateTime start, DateTime end)
        {
            var availabilityMatrices = await GetAvailabilityMatrices(start, end);

            Dictionary<string, decimal[]> resultLists = new Dictionary<string, decimal[]>();
            var unitIds = GetUnitIdsUnion(availabilityMatrices);

            foreach (var unitId in unitIds)
            {
                decimal[] array = availabilityMatrices.SelectMany(x => x.GetAvailabilityArray(unitId, start, end)).ToArray();
                resultLists.Add(unitId, array);
            }
            return resultLists;
        }

        public async Task<Dictionary<string, decimal>> GetAvailabilityAsCount(DateTime start, DateTime end)
        {
            var availabilityArrays = await this.GetAvailabilityAsArrays(start, end);
            var availablityBoolean = availabilityArrays.ToDictionary(x => x.Key, x => x.Value.Min(a => a));
            return availablityBoolean;
        }

        public async Task<bool> IsReadyForQuerying(DateTime start, DateTime end)
        {
            var existingAvailabilityMatrices = await GetExistingAvailabilityMatrices(start, end);
            var timeChunks = _timeChunkDivider.BuildTimeChunks(start, end);
            var allTimeCunksAreCreated = timeChunks.All(tc => existingAvailabilityMatrices.Exists(m => m.Id == tc.Key));
            return allTimeCunksAreCreated;
        }

        public async Task Initialize(DateTime start, DateTime end)
        {
            _availabilityMatrixRepository.ClearLocalCache();

            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            var existingAvailabilityMatrices = await GetExistingAvailabilityMatrices(start, end);

            var timeChunks = _timeChunkDivider.BuildTimeChunks(start, end);
            foreach (var timeChunk in timeChunks)
            {
                if (!existingAvailabilityMatrices.Exists(x => x.Id == timeChunk.Key))
                {
                    CreateAvailabilityMatrixAndAddToRepository(timeChunk);
                }
            }

            await SeedUnseededPartOfCache(start, end);
            await SaveChanges();
            transactionScope.Complete();
        }
          
        public async Task SeedUnseededPartOfCache(DateTime start, DateTime end)
        {
            var timeChunks = _timeChunkDivider.BuildTimeChunks(start, end);
            var keys = timeChunks.Select(x => x.Key);
            var unseededAvailabilityMatrices = (await _availabilityMatrixRepository.FindUnseeded(keys)).ToList();
            if (unseededAvailabilityMatrices.Count == 0) return;

            await _availabilitySeeder.PreFetchAllocations(timeChunks.First().Start, timeChunks.Last().End);
            foreach (var unseededAvailabilityMatrix in unseededAvailabilityMatrices)
            {
                await _availabilitySeeder.SeedMatrix(unseededAvailabilityMatrix);
            }
        }

        public async Task SaveChanges()
        {
            if (Transaction.Current == null) throw new AvailabilityException("AvailabilityCache.SaveChanges MUST happen within a transaction");
            await _availabilityMatrixRepository.SaveChangesToAppDbContext();
        }

        public void DeleteCache()
        {
            this._availabilityMatrixRepository.DeleteAll();
        }

        private List<string> GetUnitIdsUnion(List<AvailabilityMatrix> availabilityMatrices)
        {
            var unitsUnion = availabilityMatrices.SelectMany(x => x.AvailabilityArrays.Keys).Distinct().ToList();
            return unitsUnion;
        }

        private async Task<List<AvailabilityMatrix>> GetAvailabilityMatrices(DateTime start, DateTime end)
        {
            var existingAvailabilityMatrices = await GetExistingAvailabilityMatrices(start, end);
            var timeChunks = _timeChunkDivider.BuildTimeChunks(start, end);
            foreach (var timeChunk in timeChunks)
            {
                if (!existingAvailabilityMatrices.Exists(x => x.Id == timeChunk.Key))
                {
                    throw new AvailabilityCacheNotInitialized(timeChunk.Start, timeChunk.End);
                }
            }
            return existingAvailabilityMatrices.ToList();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private AvailabilityMatrix CreateAvailabilityMatrixAndAddToRepository(TimeChunk timeChunk)
        {
            var availabilityMatrix = _availabilityMatrixFactory.Create(timeChunk.Key, timeChunk.Start, timeChunk.End);
            _availabilityMatrixRepository.AddAndSave(availabilityMatrix);
            return availabilityMatrix;
        }

        private async Task<List<AvailabilityMatrix>> GetExistingAvailabilityMatrices(DateTime start, DateTime end)
        {
            var timeChunks = _timeChunkDivider.BuildTimeChunks(start, end);
            var keys = timeChunks.Select(x => x.Key);
            var availabilityMatrices = await _availabilityMatrixRepository.Find(keys);
            return availabilityMatrices.OrderBy(x => x.Start).ToList();
        }
    }
}