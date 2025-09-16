using System;
using System.Threading.Tasks;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    public interface IPaymentLinkRepository : ICloneWithCustomClaimsProvider<IPaymentLinkRepository>
    {
        Task Add(PaymentLink paymentLink);
        Task<PaymentLink> FindSingle(Guid id);
        Task<PaymentLink> FindSingleOrDefault(Guid id);
        Task<PaymentLink> FindSingleOrDefaultFromFriendlyVoucherId(long friendlyVoucherId, VoucherType voucherType);
    }
}
