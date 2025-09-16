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
    public class DiscountRepository : TenantFilteredRepository<Discount, AppDbContext>, IDiscountRepository
    {
        public DiscountRepository(IClaimsProvider claimsProvider, AppDbContext appDbContext) : base(claimsProvider, appDbContext)
        {
        }

        public void Add(Discount discount)
        {
            AppDbContext.Add(discount);
        }

        public Discount FindSingleOrDefaultFromName(string name)
        {
            var discount = this.TenantFilteredEntitiesQuery.FirstOrDefault(x => x.Name == name && !x.IsDeleted);
            return discount;
        }

        public void Update(Discount discount)
        {
            AppDbContext.Update(discount);
        }

        public async Task<IList<Discount>> FindAll()
        {
            var discounts = await this.TenantFilteredEntitiesQuery.ToListAsync();
            return discounts;
        }

        public Discount FindSingle(Guid id)
        {
            var discount = this.TenantFilteredEntitiesQuery.Single(x => x.Id == id);
            return discount;
        }

        public void MarkAsDeleted(Guid id)
        {
            var discount = FindSingle(id);
            discount.IsDeleted = true;
        }
    }
}
