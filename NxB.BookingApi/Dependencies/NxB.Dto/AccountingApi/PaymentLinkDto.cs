using System;
using System.Collections.Generic;
using System.Text;
using Munk.Utils.Object;
using NxB.Domain.Common.Enums;
using NxB.Dto.OrderingApi;

namespace NxB.Dto.AccountingApi
{
    public class BasePaymentLinkDto
    {
        public long FriendlyVoucherId { get; set; }
        public decimal Amount { get; set; }
    }

    public class CreatePaymentLinkDto : BasePaymentLinkDto
    {
        public PaymentLinkTestMode TestMode { get; set; } = PaymentLinkTestMode.None;
        public VoucherType VoucherType { get; set; }
        public long FriendlyOrderId { get; set; }
    }

    public class CreatePaymentLinkTenantDto : CreatePaymentLinkDto
    {
        public Guid TenantId { get; set; }
    }

    public class CreatePaymentLinkForPaymentVoucherDto
    {
        public Guid TenantId { get; set; }
        public PaymentLinkTestMode TestMode { get; set; } = PaymentLinkTestMode.None;
        public long FriendlyOrderId { get; set; }
        public decimal Amount { get; set; }
        public string Language { get; set; }

        [NoEmpty]
        public string HostName { get; set; }
        public string KioskId { get; set; }
    }

    public class PaymentLinkDto : BasePaymentLinkDto
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public Guid CreateAuthorId { get; set; }
        public Guid TenantId { get; set; }
        public int QuickPayPaymentId { get; set; }
        public string Url { get; set; }
    }

    public class CreateOnlinePaymentLinkDto
    {
        [NoEmpty]
        public Guid TenantId { get; set; }

        public string Language { get; set; }
        public PaymentLinkTestMode TestMode { get; set; }
        public int? ExistingPaymentId { get; set; }

        [NoEmpty]
        public string HostName { get; set; }

        public CartDto Cart { get; set; }

        [NoEmpty]
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string QuickPayOrderId { get; set; }

        public string OverrideContinueUrl { get; set; }
        public string OverrideCancelUrl { get; set; }
        public string ContinueUrlParameters { get; set; }
        public string CancelUrlParameters { get; set; }

    }

    public class TemporaryPaymentLinkDto
    {
        public DateTime CreateDate { get; set; }
        public Guid CreateAuthorId { get; set; }
        public Guid TenantId { get; internal set; }
        public int QuickPayPaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Url { get; set; }
        public long FriendlyOrderId { get; set; }
    }


    public class PaymentLinkOnlineDto
    {
        public int PaymentId { get; set; }
        public string Url { get; set; }
    }

    public enum PaymentLinkTestMode
    {
        None,
        NoCallback,
    }

    public enum LinkType
    {
        None,
        Voucher,
        Booking
    }
}
