using NxB.BookingApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Infrastructure
{
    public static class PaymentCompletionQueryExtensions
    {
        public static IQueryable<PaymentCompletion> WithoutResponseJson(
            this IQueryable<PaymentCompletion> query)
        {
            return query.Select(pc =>
                new PaymentCompletion(
                    pc.Id,
                    pc.TenantId,
                    pc.Amount,
                    pc.VoucherType,
                    pc.FriendlyOrderId,
                    pc.FriendlyPaymentId,
                    pc.FriendlyVoucherId,
                    pc.QuickPayPaymentId,
                    pc.QuickPayOrderId,
                    pc.Success,
                    default!,              // <— constant; column NOT fetched
                    pc.InitiatedByUser,
                    pc.IsFeeAdded,
                    pc.IsAutoCaptured,
                    pc.PaymentLinkId,
                    pc.State,
                    pc.TransactionType,
                    pc.IsLegacy,
                    pc.IsPending,
                    pc.LinkSource
                )
                {
                    // properties not in the ctor
                    CreateDate = pc.CreateDate,
                    IsArchived = pc.IsArchived,
                    OnCompletionAction = pc.OnCompletionAction,
                    PaymentId = pc.PaymentId
                });
        }
    }
}
