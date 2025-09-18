using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using NxB.Domain.Common.Interfaces;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class AccessGroupRepository : TenantFilteredRepository<AccessGroup, AppDbContext>, IAccessGroupRepository
    {
        public AccessGroupRepository(IClaimsProvider claimsProvider, AppDbContext appDbContext) : base(claimsProvider, appDbContext)
        {
        }

        public void Add(AccessGroup accessGroup)
        {
            AppDbContext.Add(accessGroup);
        }

        public void Update(AccessGroup accessGroup)
        {
            AppDbContext.Update(accessGroup);
        }

        public async Task<AccessGroup> MarkAsDeleted(Guid id)
        {
            var accessGroup = await this.FindSingle(id);
            accessGroup.MarkAsDeleted();
            return accessGroup;
        }

        public Task<AccessGroup> FindSingle(Guid id)
        {
            return this.TenantFilteredEntitiesQuery.SingleAsync(x => x.Id == id);
        }

        public static Task<AccessGroup> FindSingle(Guid id, AppDbContext appDbContext)
        {
            return appDbContext.AccessGroups.SingleAsync(x => x.Id == id);
        }

        public Task<List<AccessGroup>> FindAll()
        {
            return this.TenantFilteredEntitiesQuery.Where(x => !x.IsDeleted).OrderBy(x => x.Sort).OrderBy(x => x.Name).ToListAsync();
        }
    }
}