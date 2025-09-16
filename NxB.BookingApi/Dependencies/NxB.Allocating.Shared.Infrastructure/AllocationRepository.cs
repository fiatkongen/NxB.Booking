using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using Munk.Utils.Object;
using NxB.Allocating.Shared.Model;
using NxB.Domain.Common.Interfaces;

namespace NxB.Allocating.Shared.Infrastructure
{
    public class AllocationRepository<TAppDbContext> : TenantFilteredRepository<Allocation, TAppDbContext>, IAllocationRepository where TAppDbContext : DbContext
    {
        public AllocationRepository(IClaimsProvider claimsProvider, TAppDbContext appDbContext) : base(claimsProvider, appDbContext)
        {
        }

        public void Add(Allocation allocation)
        {
            this.AppDbContext.Add(allocation);
        }

        public async Task Add(IEnumerable<Allocation> allocations)
        {
            await this.AppDbContext.AddRangeAsync(allocations);
        }

        public async Task<Allocation> FindSingle(Guid id)
        {
            var allocation = await this.TenantFilteredEntitiesQuery.SingleAsync(x => x.Id == id);
            return allocation;
        }

        public async Task<List<Allocation>> FindAll()
        {
            var allocations = await this.TenantFilteredEntitiesQuery.ToListAsync();
            return allocations.ToList();
        }

        public async Task<List<Guid>> FindAllRentalUnitIds()
        {
            var ids = await this.TenantFilteredEntitiesQuery.Select(x => x.RentalUnitId).Distinct().ToListAsync();
            return ids;
        }

        public async Task<List<Allocation>> FindAllWithRentalUnitIds(IEnumerable<Guid> rentalUnitIds)
        {
            var allocations = await this.TenantFilteredEntitiesQuery.Where(x => rentalUnitIds.Contains(x.RentalUnitId)).ToListAsync();
            return allocations.ToList();
        }

        public async Task DeleteAllocationsForRentalUnitIds(IEnumerable<Guid> rentalUnitIds)
        {
            var allocations = await this.FindAllWithRentalUnitIds(rentalUnitIds);
            //var allocations = list.Join(rentalUnitIds.AsQueryable(), a => a.RentalUnitId, ids => ids, (a, ids) => a).ToList();
            AppDbContext.RemoveRange(allocations);
        }

        public void DeleteAllocationsForTenant(Guid tenantId)
        {
            var allocations = this.AppDbContext.Set<Allocation>().Where(x => x.TenantId == tenantId).ToList();
            AppDbContext.RemoveRange(allocations);
        }

        public void DeleteAllocation(Guid id)
        {
            var allocation = this.AppDbContext.Set<Allocation>().Single(x => x.Id == id);
            this.AppDbContext.Set<Allocation>().Remove(allocation);
        }

        public async Task<List<Allocation>> FindWithinInterval(DateTime start, DateTime end)
        {
            var query = this.TenantFilteredEntitiesQuery.OverlapsWith(new DateInterval(start, end), x => x.Start, x => x.End);
            var allocations = await query.ToListAsync();
            return allocations;
        }

        public async Task<List<Allocation>> FindOccupationsWithinInterval(DateTime start, DateTime end)
        {
            var query = this.TenantFilteredEntitiesQuery.Where(x => x.Number < 0).OverlapsWith(new DateInterval(start, end), x => x.Start, x => x.End);
            var allocations = await query.ToListAsync();
            return allocations;
        }

        public async Task<List<Allocation>> FindWithinIntervalForUnit(DateTime start, DateTime end, Guid unitId)
        {
            var query = this.TenantFilteredEntitiesQuery.OverlapsWith(new DateInterval(start, end), x => x.Start, x => x.End).Where(x => x.RentalUnitId == unitId);
            var allocations = await query.ToListAsync();
            return allocations;
        }
    }
}
