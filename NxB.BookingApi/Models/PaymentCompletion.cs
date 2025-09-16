using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using NxB.BookingApi.Exceptions;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.AccountingApi;

namespace NxB.BookingApi.Models
{
    public class PaymentCompletion : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now.ToEuTimeZone();
        public decimal Amount { get; set; }
        public VoucherType? VoucherType { get; set; }
        public long FriendlyOrderId { get; set; }
        public long? FriendlyPaymentId { get; set; }
        public long? FriendlyVoucherId { get; set; }
        public int QuickPayPaymentId { get; set; }
        public string QuickPayOrderId { get; set; }
        public bool Success { get; set; }
        public string ResponseJson { get; set; }
        public Guid? InitiatedByUser { get; set; }
        public bool IsFeeAdded { get; set; }
        public bool IsAutoCaptured { get; set; }
        public Guid? PaymentLinkId { get; set; }
        public string State { get; set; }
        public string TransactionType { get; set; }
        public bool IsArchived { get; set; }
        public PaymentCompletionAction OnCompletionAction { get; set; } = PaymentCompletionAction.None;
        public Guid? PaymentId { get; set; }
        public bool IsLegacy { get; set; }
        public bool IsPending { get; set; }
        public LinkSourceType LinkSource { get; set; }

        private PaymentCompletion(){}

        public PaymentCompletion(Guid id, Guid tenantId, decimal amount, VoucherType? voucherType, long friendlyOrderId, long? friendlyPaymentId, long? friendlyVoucherId, int quickPayPaymentId, string quickPayOrderId, bool success, string responseJson, Guid? initiatedByUser, bool isFeeAdded, bool isAutoCaptured, Guid? paymentLinkId, string state, string transactionType, bool isLegacy, bool isPending, LinkSourceType linkSource)
        {
            Id = id;
            TenantId = tenantId;
            Amount = amount;
            VoucherType = voucherType;
            FriendlyOrderId = friendlyOrderId;
            FriendlyPaymentId = friendlyPaymentId;
            FriendlyVoucherId = friendlyVoucherId;
            QuickPayPaymentId = quickPayPaymentId;
            QuickPayOrderId = quickPayOrderId;
            Success = success;
            ResponseJson = responseJson;
            InitiatedByUser = initiatedByUser;
            IsFeeAdded = isFeeAdded;
            IsAutoCaptured = isAutoCaptured;
            PaymentLinkId = paymentLinkId;
            State = state;
            TransactionType = transactionType;
            IsLegacy = isLegacy;
            IsPending = isPending;
            LinkSource = linkSource;
        }
    }
}
