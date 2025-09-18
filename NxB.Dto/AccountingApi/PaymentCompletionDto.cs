using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Enums;

namespace NxB.Dto.AccountingApi
{
    public class CreatePaymentCompletionDto
    {
        public Guid TenantId { get; set; }
        public DateTime CreateDate { get; set; }
        public decimal Amount { get; set; }
        public VoucherType VoucherType { get; set; }
        public long FriendlyOrderId { get; set; }
        public long? FriendlyVoucherId { get; set; }
        public long? FriendlyPaymentId { get; set; }
        public int QuickPayPaymentId { get; set; }
        public string QuickPayOrderId { get; set; }
        public bool Success { get; set; }
        public string ResponseJson { get; set; }
        public Guid? InitiatedByUser { get; set; }
        public bool IsFeeAdded { get; set; }
        public bool IsAutoCaptured { get; set; }
        public Guid PaymentLinkId { get; set; }
        public string State { get; set; }
        public bool IsLegacy { get; set; }
        public bool IsPending { get; set; }
        public bool IsArchived { get; set; }
        public string TransactionType { get; set; }
        public LinkSourceType LinkSource { get; set; }
    }

    public class PaymentCompletionDto : CreatePaymentCompletionDto
    {
        public Guid Id { get; set; }
       
    }

    public enum LinkSourceType
    {
        OnlineBooking = 0,
        PaymentLink = 1,
        DkCamp = 2
    }
}
