using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using NxB.Domain.Common.Interfaces;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class AccessRepository : TenantFilteredRepository<Access, AppDbContext>, IAccessRepository
    {
        public AccessRepository(IClaimsProvider claimsProvider, AppDbContext appDbContext) : base(claimsProvider, appDbContext)
        {
        }

        public void Add(Access access)
        {
            AppDbContext.Add(access);
        }

        public async Task Remove(Guid id)
        {
            var access = await FindAccess(id);
            AppDbContext.Remove(access);
        }

        public Task<Access> FindAccess(Guid id)
        {
            return this.TenantFilteredEntitiesQuery.SingleAsync(x => x.Id == id);
        }

        public Task<Access> FindSingleOrDefaultAccess(Guid id)
        {
            return this.TenantFilteredEntitiesQuery.SingleOrDefaultAsync(x => x.Id == id);
        }

        public Task<Access> FindActiveAccessFromCode(int code)
        {
            return this.TenantFilteredEntitiesQuery.SingleOrDefaultAsync(x => x.Code == code && !x.IsDeleted && x.DeactivationDate == null);
        }

        public Task<Access> FindAutoActivationAccessFromCode(int code)
        {
            return this.TenantFilteredEntitiesQuery.SingleOrDefaultAsync(x => x.Code == code && !x.IsDeleted && x.AutoActivationDate != null);
        }

        public Task<Access> FindAccessOrDefault(Guid id)
        {
            return this.TenantFilteredEntitiesQuery.SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Access> MarkAsDeleted(Guid id)
        {
            var access = await this.FindAccess(id);
            access.MarkAsDeleted();
            return access;
        }

        public async Task<Access> DeactivateFromCode(int code)
        {
            var existingAccess = await FindActiveAccessFromCode(code);
            if (existingAccess != null)
            {
                return await this.Deactivate(existingAccess.Id);
            }

            return null;
        }

        public async Task<Access> Reactivate(Guid id)
        {
            var access = await this.FindAccess(id);
            access.Reactivate();
            return access;
        }

        public async Task<Access> Deactivate(Guid id)
        {
            var access = await this.FindAccess(id);
            access.Deactivate();
            return access;
        }

        public Task<List<Access>> FindAllActive()
        {
            return this.TenantFilteredEntitiesQuery.Where(x => x.DeactivationDate == null && !x.IsDeleted).OrderBy(x => x.CreateDate).ToListAsync();
        }

        public Task<List<Access>> FindAllInActive(DateTime deactivationDate)
        {
            return this.TenantFilteredEntitiesQuery.Where(x => x.DeactivationDate != null && x.DeactivationDate >= deactivationDate && !x.IsDeleted).OrderBy(x => x.CreateDate).ToListAsync();
        }

        public Task<List<Access>> FindFromSubOrderId(Guid subOrderId, bool? isKeyCode = null)
        {
            return this.TenantFilteredEntitiesQuery.Where(x => x.SubOrderId == subOrderId && (isKeyCode == null || x.IsKeyCode == isKeyCode.Value) ).OrderBy(x => x.CreateDate).ToListAsync();
        }
    }
}
