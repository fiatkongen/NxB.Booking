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
    public class AutoAddRepository : TenantFilteredRepository<AutoAddState, AppDbContext>, IAutoAddRepository
    {
        public AutoAddRepository(IClaimsProvider claimsProvider, AppDbContext appDbContext) : base(claimsProvider, appDbContext)
        {
        }

        public void Add(AutoAdd autoAdd)
        {
            this.AppDbContext.AutoAdds.Add(new AutoAddState(this.ClaimsProvider.GetTenantId(), autoAdd, 0));
        }

        public void Delete(Guid id)
        {
            var autoAddState = TenantFilteredEntitiesQuery.Single(x => x.Id == id);
            AppDbContext.Remove(autoAddState);
        }

        public AutoAdd FindSingle(Guid id)
        {
            var autoAddState = TenantFilteredEntitiesQuery.Single(x => x.Id == id);
            return autoAddState.GetModel();
        }

        public Task<List<AutoAdd>> FindAll()
        {
            var autoAdds = TenantFilteredEntitiesQuery.Select(x => x.GetModel()).ToListAsync();
            return autoAdds;
        }
    }
}
