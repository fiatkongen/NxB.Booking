using Munk.AspNetCore;
using NxB.BookingApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Infrastructure
{
    public class ExternalPaymentTransactionRepository : TenantFilteredRepository<ExternalPaymentTransaction, AppDbContext>, IExternalPaymentTransactionRepository
    {
        public ExternalPaymentTransactionRepository(IClaimsProvider claimsProvider, AppDbContext appDbContext) : base(claimsProvider, appDbContext)
        {
        }

        public void Add(ExternalPaymentTransaction externalPaymentTransaction)
        {
            AppDbContext.Add(externalPaymentTransaction);
        }

        public Task<ExternalPaymentTransaction> FindSingleOrDefault(Guid id)
        {
            return TenantFilteredEntitiesQuery.SingleOrDefaultAsync(x => x.Id == id);
        }

        public Task<List<ExternalPaymentTransaction>> FindAll()
        {
            return TenantFilteredEntitiesQuery.ToListAsync();
        }

        public Task<List<ExternalPaymentTransaction>> FindAllFromVoucherId(Guid voucherId)
        {
            return TenantFilteredEntitiesQuery.Where(x => x.VoucherId == voucherId).ToListAsync();
        }
    }
}