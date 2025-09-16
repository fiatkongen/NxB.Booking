using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Infrastructure
{
    public class PaymentCompletionRepository : TenantFilteredRepository<PaymentCompletion, AppDbContext>, IPaymentCompletionRepository
    {
        public PaymentCompletionRepository(IClaimsProvider claimsProvider, AppDbContext appDbContext) : base(claimsProvider, appDbContext)
        {
        }

        public IPaymentCompletionRepository CloneWithCustomClaimsProvider(IClaimsProvider overrideClaimsProvider)
        {
            return new PaymentCompletionRepository(overrideClaimsProvider, AppDbContext);
        }

        public async Task Add(PaymentCompletion paymentCompletion)
        {
            await AppDbContext.AddAsync(paymentCompletion);
        }

        public void Update(PaymentCompletion paymentCompletion)
        {
            AppDbContext.Update(paymentCompletion);
        }

        public void Delete(PaymentCompletion paymentCompletion)
        {
            AppDbContext.Remove(paymentCompletion);
        }

        public async Task<List<PaymentCompletion>> FindAll(DateInterval dateInterval, bool includeArchived)
        {
            var paymentCompletions = await TenantFilteredEntitiesQuery.Where(x => x.CreateDate >= dateInterval.Start && x.CreateDate <= dateInterval.End && !x.IsPending && (includeArchived || x.IsArchived == false)).OrderByDescending(x => x.CreateDate)
                .AsNoTracking()              // recommended for projections
                .WithoutResponseJson()
                .ToListAsync();
            return paymentCompletions;
        }

        public async Task<PaymentCompletion> FindSingleOrDefaultFromFriendlyOrderId(long friendlyOrderId, string transactionType)
        {
            return await TenantFilteredEntitiesQuery.SingleOrDefaultAsync(x => x.FriendlyOrderId == friendlyOrderId && x.TransactionType == transactionType);
        }

        public async Task<PaymentCompletion> FindSingleOrDefaultFromQuickPayOrderId(int quickPayId, string transactionType)
        {
            return await TenantFilteredEntitiesQuery.SingleOrDefaultAsync(x => x.QuickPayPaymentId == quickPayId && x.TransactionType == transactionType);
        }

        public Task<List<PaymentCompletion>> FindAllPending_Global(DateTime createDateBefore)
        {
            return this.AppDbContext.PaymentCompletions.Where(x => x.IsPending && x.CreateDate < createDateBefore).ToListAsync();
        }

        public Task<List<PaymentCompletion>> FindAllIncludePending_Global(bool includeArchived)
        {
            return this.AppDbContext.PaymentCompletions.Where(x => includeArchived || x.IsArchived == false).ToListAsync();
        }

        public async Task ArchiveAllCompleted()
        {
            var notCompletedPaymentCompletions = await TenantFilteredEntitiesQuery.Where(x => x.IsArchived == false && !x.IsPending && x.State == "processed").ToListAsync();
            notCompletedPaymentCompletions.ToList().ForEach(x => { x.IsArchived = true; this.Update(x); });
        }

        public async Task RemovePending(long friendlyOrderId)
        {
            var pendingPaymentCompletion = await TenantFilteredEntitiesQuery.SingleOrDefaultAsync(x => x.IsPending && x.FriendlyOrderId == friendlyOrderId);
            if (pendingPaymentCompletion != null)
            {
                pendingPaymentCompletion.IsPending = false;
            }
        }

        public Task<int> CountActive()
        {
            return TenantFilteredEntitiesQuery.CountAsync(x => !x.IsArchived && !x.IsPending);
        }

        public async Task<PaymentCompletion> FindSingle(Guid id)
        {
            return await TenantFilteredEntitiesQuery.SingleAsync(x => x.Id == id);
        }

        public async Task<PaymentCompletion> FindSingleOrDefault(Guid id)
        {
            return await TenantFilteredEntitiesQuery.SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<PaymentCompletion> FindSingleOrDefaultFromFriendlyVoucherId(long friendlyVoucherId, VoucherType voucherType, string transactionType)
        {
            return await TenantFilteredEntitiesQuery.SingleOrDefaultAsync(x => x.FriendlyPaymentId == friendlyVoucherId && x.VoucherType == voucherType && x.TransactionType == transactionType);
        }

        public async Task<PaymentCompletion> FindSingleOrDefaultFromPaymentId(Guid paymentId)
        {
            return await TenantFilteredEntitiesQuery.SingleOrDefaultAsync(x => x.PaymentId == paymentId);
        }

        public async Task<PaymentCompletion> FindSingleOrDefaultFromQuickPayPaymentId(int quickPayPaymentId, string transactionType)
        {
            return await AppDbContext.PaymentCompletions.SingleOrDefaultAsync(x => x.QuickPayPaymentId == quickPayPaymentId && x.TransactionType == transactionType);
        }
    }
}
