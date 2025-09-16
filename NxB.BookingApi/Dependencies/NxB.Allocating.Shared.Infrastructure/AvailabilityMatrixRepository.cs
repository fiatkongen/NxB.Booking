using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Itenso.TimePeriod;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using Munk.Utils.Object;
using NxB.Allocating.Shared.Model;
using NxB.Domain.Common.Interfaces;

namespace NxB.Allocating.Shared.Infrastructure
{
    public class AvailabilityMatrixRepository<TAppDbContext> : TenantFilteredRepository<AvailabilityMatrix, TAppDbContext>, IAvailabilityMatrixRepository where TAppDbContext : DbContext
    {
        public AvailabilityMatrixRepository(IClaimsProvider claimsProvider, IAppDbContextFactory<TAppDbContext> appDbContextFactory) : base(claimsProvider, appDbContextFactory.Create("AvailabilityMatrixRepository"))
        { }

        public async Task<AvailabilityMatrix> FindSingleOrDefault(string key)
        {
            var availabilityMatrix = await this.TenantFilteredEntitiesQuery.SingleOrDefaultAsync(x => x.Id == key);
            return availabilityMatrix;
        }

        public async Task<IList<AvailabilityMatrix>> Find(IEnumerable<string> keys)
        {
            var predicate = BuildKeysPredicate(keys);
            var availabilityMatrices = await TenantFilteredEntitiesQuery.Where(predicate).ToArrayAsync();
            return availabilityMatrices;
        }

        public async Task<IList<AvailabilityMatrix>> FindUnseeded(IEnumerable<string> keys)
        {
            var predicate = BuildKeysPredicate(keys);
            var availabilityMatrices = await this.TenantFilteredEntitiesQuery.Where(x => !x.IsSeeded).Where(predicate).ToArrayAsync();
            return availabilityMatrices;
        }

        //public async Task ShrinkAllocationResources(List<string> validResourceIds)
        //{
        //    var availabilityMatrices = await this.FindAll();
        //    foreach (var availabilityMatrix in availabilityMatrices)
        //    {
        //        availabilityMatrix.ShrinkAvailabilityArrays(validResourceIds);
        //    }
        //}

        private static Expression<Func<AvailabilityMatrix, bool>> BuildKeysPredicate(IEnumerable<string> keys)
        {
            var predicate = PredicateBuilder.False<AvailabilityMatrix>();
            foreach (var key in keys)
            {
                predicate = predicate.Or(x => x.Id == key);
            }

            return predicate;
        }

        public async Task<IList<AvailabilityMatrix>> FindAll()
        {
            var availabilityMatrices = await this.TenantFilteredEntitiesQuery.ToArrayAsync();
            return availabilityMatrices;
        }

        public async Task SaveChangesToAppDbContext()
        {
            await this.AppDbContext.SaveChangesAsync();
        }

        public void DeleteAll()
        {
            var availabilityMatrices = this.TenantFilteredEntitiesQuery.ToList();
            this.AppDbContext.RemoveRange(availabilityMatrices);
            this.AppDbContext.SaveChanges();
        }

        public void ClearLocalCache()
        {
            this.AppDbContext.ClearDbContextCache();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddAndSave(AvailabilityMatrix availabilityMatrix)
        {
            if (this.AppDbContext.Set<AvailabilityMatrix>().Where(x => x.TenantId == TenantId).None(x => x.Id == availabilityMatrix.Id))
            {
                this.AppDbContext.Add(availabilityMatrix);
            }
            else
            {
                this.AppDbContext.Update(availabilityMatrix);
            }

            this.AppDbContext.SaveChanges();
        }
    }
}
