using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Itenso.TimePeriod;
using Munk.Utils.Object;
using NxB.Allocating.Shared.Model;

namespace NxB.Allocating.Shared.Infrastructure
{
    public class AllocationRepositoryCached : IAllocationRepositoryCached
    {
        private readonly IAllocationRepository _allocationRepository;
        private List<Allocation> _cachedAllocations;
        private TimeBlock _cachedTimeBlock;

        public AllocationRepositoryCached(IAllocationRepository allocationRepository)
        {
            _allocationRepository = allocationRepository;
        }

        public async Task PreFectAllocations(DateTime start, DateTime end)
        {
            start = start.Date;
            end = end.Date;

            _cachedAllocations = await _allocationRepository.FindWithinInterval(start, end);
            _cachedTimeBlock = new TimeBlock(start, end);
        }

        public void Add(Allocation allocation)
        {
            _allocationRepository.Add(allocation);
        }

        public async Task Add(IEnumerable<Allocation> allocations)
        {
            await _allocationRepository.Add(allocations);
        }

        public async Task<Allocation> FindSingle(Guid id)
        {
            return await _allocationRepository.FindSingle(id);
        }

        public async Task<List<Allocation>> FindWithinInterval(DateTime start, DateTime end)
        {
            start = start.Date;
            end = end.Date;

            if (_cachedTimeBlock != null && _cachedTimeBlock.HasInside(new TimeBlock(start, end)))
            {
                var allocations = _cachedAllocations.AsQueryable().OverlapsWith(new DateInterval(start, end), x => x.Start, x => x.End).ToList();
                return allocations;
            }
            return await _allocationRepository.FindWithinInterval(start, end);
        }

        public async Task<List<Allocation>> FindOccupationsWithinInterval(DateTime start, DateTime end)
        {
            var allocations = await FindWithinInterval(start, end);
            allocations = allocations.Where(x => x.Number < 0).ToList();
            return allocations;
        }


        public async Task<List<Allocation>> FindWithinIntervalForUnit(DateTime start, DateTime end, Guid unitId)
        {
            return await _allocationRepository.FindWithinIntervalForUnit(start, end, unitId);
        }

        Task<List<Allocation>> IAllocationRepository.FindAll()
        {
            throw new NotImplementedException();
        }

        public void DeleteAllocationsForTenant(Guid tenantId)
        {
            throw new NotImplementedException();
        }

        public void DeleteAllocation(Guid id)
        {
            throw new NotImplementedException();
        }


        //todo: segregate interface
        public Task<List<Guid>> FindAllRentalUnitIds()
        {
            throw new NotImplementedException();
        }

        public Task<List<Allocation>> FindAllWithRentalUnitIds(IEnumerable<Guid> rentalUnitIds)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<Allocation>> FindAll()
        {
            return await _allocationRepository.FindAll();
        }

        Task IAllocationRepository.DeleteAllocationsForRentalUnitIds(IEnumerable<Guid> rentalUnitIds)
        {
            throw new NotImplementedException();
        }

    }
}