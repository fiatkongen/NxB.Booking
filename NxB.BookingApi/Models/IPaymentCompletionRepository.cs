using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    public interface IPaymentCompletionRepository : ICloneWithCustomClaimsProvider<IPaymentCompletionRepository>
    {
        Task Add(PaymentCompletion paymentCompletion);
        void Update(PaymentCompletion paymentCompletion);
        void Delete(PaymentCompletion paymentCompletion);
        Task<List<PaymentCompletion>> FindAll(DateInterval dateInterval, bool includeArchived);
        Task ArchiveAllCompleted();
        Task RemovePending(long friendlyOrderId);
        Task<int> CountActive();
        Task<PaymentCompletion> FindSingle(Guid id);
        Task<PaymentCompletion> FindSingleOrDefault(Guid id);
        Task<PaymentCompletion> FindSingleOrDefaultFromFriendlyVoucherId(long friendlyVoucherId, VoucherType voucherType, string transactionType);
        Task<PaymentCompletion> FindSingleOrDefaultFromPaymentId(Guid paymentId);

        Task<PaymentCompletion> FindSingleOrDefaultFromQuickPayPaymentId(int quickPayPaymentId, string transactionType);
        Task<PaymentCompletion> FindSingleOrDefaultFromFriendlyOrderId(long friendlyOrderId, string transactionType);
        Task<PaymentCompletion> FindSingleOrDefaultFromQuickPayOrderId(int quickPayId, string transactionType);
        Task<List<PaymentCompletion>> FindAllPending_Global(DateTime createDateBefore);
        
        //to be removed
        Task<List<PaymentCompletion>> FindAllIncludePending_Global(bool includeArchived);
    }
}
