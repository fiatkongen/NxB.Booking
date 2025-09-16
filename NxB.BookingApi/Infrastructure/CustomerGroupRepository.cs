using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Infrastructure
{
    public class CustomerGroupRepository : TenantFilteredRepository<Customer, AppDbContext>, ICustomerGroupRepository
    {
        public CustomerGroupRepository(IClaimsProvider claimsProvider, AppDbContext appDbContext) : base(claimsProvider, appDbContext)
        {
        }

        public void Add(CustomerGroup customerGroup)
        {
            AppDbContext.Add(customerGroup);
        }

        public void MarkAsDeleted(Guid id)
        {
            var customerGroup = FindSingle(id);
            if (customerGroup != null)
                customerGroup.IsDeleted = true;
        }

        public CustomerGroup FindSingle(Guid id)
        {
            var customerGroup = AppDbContext.CustomerGroups.Single(x => x.Id == id);
            return customerGroup;
        }

        public Task<List<CustomerGroup>> FindAll()
        {
            return AppDbContext.CustomerGroups.Where(x => x.TenantId == ClaimsProvider.GetTenantId()).OrderBy(x => x.Name).ToListAsync();
        }
    }
}
