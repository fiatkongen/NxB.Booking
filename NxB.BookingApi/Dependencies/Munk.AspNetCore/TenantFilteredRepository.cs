using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NxB.Domain.Common.Interfaces;

namespace Munk.AspNetCore
{
    public abstract class TenantFilteredRepository<TEntityType, TAppDbContext> : BaseRepository<TAppDbContext> where TEntityType : class, ITenantEntity where TAppDbContext : DbContext
    {
        protected IClaimsProvider ClaimsProvider { get; private set; }
        protected Guid TenantId => ClaimsProvider.GetTenantId();

        protected TenantFilteredRepository(IClaimsProvider claimsProvider, TAppDbContext appDbContext) : base(appDbContext)
        {
            ClaimsProvider = claimsProvider;
        }

        protected virtual IQueryable<TEntityType> TenantFilteredEntitiesQuery
        {
            get { return Set().Where(x => x.TenantId == TenantId); }
        }

        
        protected DbSet<TEntityType> Set()
        {
            return this.AppDbContext.Set<TEntityType>();
        }
    }

    public abstract class BaseRepository<TAppDbContext> where TAppDbContext : DbContext
    {
        protected TAppDbContext AppDbContext { get; private set; }

        protected BaseRepository(TAppDbContext appDbContext)
        {
            AppDbContext = appDbContext;
        }

        protected virtual IQueryable<TEntityTypeFilter> GetTenantFilteredEntitiesQuery<TEntityTypeFilter>(Guid tenantId) where TEntityTypeFilter : class, ITenantEntity
        {
            return this.AppDbContext.Set<TEntityTypeFilter>().Where(x => x.TenantId == tenantId);
        }
    }
}
