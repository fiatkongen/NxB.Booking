using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using NxB.Domain.Common.Interfaces;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class PriceProfileRepository : TenantFilteredRepository<PriceProfile, AppDbContext>, IPriceProfileRepository
    {
        public PriceProfileRepository(IClaimsProvider claimsProvider, AppDbContext appDbContext) : base(claimsProvider, appDbContext)
        {
        }

        public IPriceProfileRepository CloneWithCustomClaimsProvider(IClaimsProvider overrideClaimsProvider)
        {
            return new PriceProfileRepository(overrideClaimsProvider, AppDbContext);
        }

        public void Update(PriceProfile priceProfile)
        {
            AppDbContext.Update(priceProfile);
        }

        public void Add(PriceProfile priceProfile)
        {
            AppDbContext.Add(priceProfile);
        }

        public void Add(IEnumerable<PriceProfile> priceProfiles)
        {
            priceProfiles.ToList().ForEach(Add);
        }

        public async Task<List<PriceProfile>> FindAll()
        {
            var priceProfiles = await this.TenantFilteredEntitiesQuery.Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync();
            return priceProfiles;
        }

        public async Task<List<PriceProfile>> FindAllFromTenantId(Guid tenantId, bool includeDeleted)
        {
            var priceProfiles = await this.AppDbContext.PriceProfiles.Where(x => tenantId == x.TenantId && (includeDeleted || !x.IsDeleted)).OrderBy(x => x.Name).ToListAsync();
            return priceProfiles;
        }

        public async Task<List<PriceProfile>> FindAllIncludeDeleted()
        {
            var priceProfiles = await this.TenantFilteredEntitiesQuery.OrderBy(x => x.Name).ToListAsync();
            return priceProfiles;
        }

        public async Task<List<PriceProfile>> FindFromResourceId(Guid resourceId)
        {
            var priceProfiles = await this.TenantFilteredEntitiesQuery.Where(x => !x.IsDeleted && x.ResourceId == resourceId).OrderBy(x => x.Name).ToListAsync();
            return priceProfiles;
        }

        public Task<PriceProfile> FindSingleOrDefaultFromResourceId(Guid resourceId, string ppName)
        {
            return this.TenantFilteredEntitiesQuery.FirstAsync(x => !x.IsDeleted && x.ResourceId == resourceId && x.Name == ppName);
        }

        public Task<List<PriceProfile>> FindFromIds(List<Guid> ids)
        {
            return this.TenantFilteredEntitiesQuery.Where(x => ids.Contains(x.Id)).ToListAsync();
        }

        public PriceProfile FindSingle(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException(nameof(id));
            var priceProfile = this.TenantFilteredEntitiesQuery.Single(x => x.Id == id);
            return priceProfile;
        }

        public PriceProfile FindSingleOrDefault(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException(nameof(id));
            var priceProfile = this.AppDbContext.PriceProfiles.SingleOrDefault(x => x.Id == id);
            return priceProfile;
        }

        public void Delete(Guid id)
        {
            var priceProfile = FindSingle(id);
            this.AppDbContext.PriceProfiles.Remove(priceProfile);
        }

        public void DeleteForResourceId(Guid resourceId)
        {
            if (resourceId == Guid.Empty) throw new ArgumentException(nameof(resourceId));
            var priceProfiles = this.TenantFilteredEntitiesQuery.Where(x => x.ResourceId == resourceId);
            AppDbContext.RemoveRange(priceProfiles);
        }

        public void MarkAsDeleted(Guid id)
        {
            var priceProfile = FindSingle(id);
            priceProfile.IsDeleted = true;
            Update(priceProfile);
        }
    }
}