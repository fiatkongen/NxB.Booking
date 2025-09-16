using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Infrastructure
{
    public class PaymentLinkRepository : TenantFilteredRepository<PaymentLink, AppDbContext>, IPaymentLinkRepository
    {
        public PaymentLinkRepository(IClaimsProvider claimsProvider, AppDbContext appDbContext) : base(claimsProvider, appDbContext)
        {
        }

        public IPaymentLinkRepository CloneWithCustomClaimsProvider(IClaimsProvider overrideClaimsProvider)
        {
            return new PaymentLinkRepository(overrideClaimsProvider, AppDbContext);
        }

        public async Task Add(PaymentLink paymentLink)
        {
            await AppDbContext.AddAsync(paymentLink);
        }

        public async Task<PaymentLink> FindSingle(Guid id)
        {
            return await TenantFilteredEntitiesQuery.SingleAsync(x => x.Id == id);
        }

        public async Task<PaymentLink> FindSingleOrDefault(Guid id)
        {
            return await TenantFilteredEntitiesQuery.SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<PaymentLink> FindSingleOrDefaultFromFriendlyVoucherId(long friendlyVoucherId, VoucherType voucherType)
        {
            return await TenantFilteredEntitiesQuery.SingleOrDefaultAsync(x => x.FriendlyVoucherId == friendlyVoucherId && x.VoucherType == voucherType);
        }
    }
}
 