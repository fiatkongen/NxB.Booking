using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using Munk.Utils.Object;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Interfaces;

namespace NxB.Allocating.Shared.Infrastructure
{
    public class TimeSpanRepository<TAppDbContext> : TenantFilteredRepository<TimeSpanBase, TAppDbContext>, ITimeSpanRepository where TAppDbContext : DbContext
    {
        public TimeSpanRepository(IClaimsProvider claimsProvider, TAppDbContext appDbContext) : base(claimsProvider, appDbContext)
        {
        }

        public void Add(TimeSpanBase timeSpan)
        {
            this.AppDbContext.Add(timeSpan);
        }

        public void Update(TimeSpanBase timeSpan)
        {
            this.AppDbContext.Update(timeSpan);
        }

        public void DeletePermanently(TimeSpanBase timeSpan)
        {
            this.AppDbContext.Remove(timeSpan);
        }

        public Task<T> FindSingleOrDefault<T>(Guid id) where T : TimeSpanBase
        {
            return this.TenantFilteredEntitiesQuery.OfType<T>().FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<List<T>> FindAll<T>() where T : TimeSpanBase
        {
            return this.TenantFilteredEntitiesQuery.OfType<T>().OrderBy(x => x.Start).ToListAsync();
        }

        public Task<List<T>> FindAllWithin<T>(DateInterval dateInterval) where T : TimeSpanBase
        {
            return this.TenantFilteredEntitiesQuery.OfType<T>().OverlapsWith(dateInterval, x => x.Start, x => x.End).OrderBy(x => x.Start).ToListAsync();
        }

        public Task<List<T>> FindAllWithinForTenant<T>(Guid tenantId, DateInterval dateInterval) where T : TimeSpanBase
        {
            return this.AppDbContext.Set<T>().Where(x => x.TenantId == tenantId).OfType<T>().OverlapsWith(dateInterval, x => x.Start, x => x.End).OrderBy(x => x.Start).ToListAsync();
        }

        public ITimeSpanRepository CloneWithCustomClaimsProvider(IClaimsProvider overrideClaimsProvider)
        {
            return new TimeSpanRepository<TAppDbContext>(overrideClaimsProvider, this.AppDbContext);
        }
    }
}
