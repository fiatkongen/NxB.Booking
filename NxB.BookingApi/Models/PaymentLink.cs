using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class TemporaryPaymentLink
    {
        public DateTime CreateDate { get; set; } = DateTime.Now.ToEuTimeZone();
        public Guid CreateAuthorId { get; set; }
        public Guid TenantId { get; internal set; }
        public int QuickPayPaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Url { get; set; }
        public long FriendlyOrderId { get; set; }

        public TemporaryPaymentLink(Guid createAuthorId, Guid tenantId, long friendlyOrderId, int quickPayPaymentId, decimal amount, string url)
        {
            CreateAuthorId = createAuthorId;
            TenantId = tenantId;
            FriendlyOrderId = friendlyOrderId;
            QuickPayPaymentId = quickPayPaymentId;
            Amount = amount;
            Url = url;
        }
    }

    [Serializable]
    public class PaymentLink : TemporaryPaymentLink, ITenantEntity
    {
        public Guid Id { get; internal set; }
        public long FriendlyVoucherId { get; set; }
        public VoucherType VoucherType { get; set; }
        
        public PaymentLink(Guid id, Guid createAuthorId, Guid tenantId, long friendlyVoucherId, VoucherType voucherType, long friendlyOrderId, int quickPayPaymentId, decimal amount, string url): base(createAuthorId, tenantId, friendlyOrderId, quickPayPaymentId, amount, url)
        {
            Id = id;
            CreateAuthorId = createAuthorId;
            TenantId = tenantId;
            FriendlyVoucherId = friendlyVoucherId;
            VoucherType = voucherType;
            QuickPayPaymentId = quickPayPaymentId;
            Amount = amount;
            Url = url;
        }
    }
}
