using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class PaymentLinkFactory
    {
        private readonly IClaimsProvider _claimsProvider;

        public PaymentLinkFactory(IClaimsProvider claimsProvider)
        {
            _claimsProvider = claimsProvider;
        }

        public PaymentLink Create(Guid id, long friendlyVoucherId, VoucherType voucherType, long friendlyOrderId, int quickPayPaymentId, decimal amount, string url)
        {
            var paymentLink = new PaymentLink(id, _claimsProvider.GetUserId(), _claimsProvider.GetTenantId(), friendlyVoucherId, voucherType, friendlyOrderId, quickPayPaymentId, amount, url);
            return paymentLink;
        }

        public TemporaryPaymentLink CreateTemporaryPaymentLink(long friendlyOrderId, int quickPayPaymentId, decimal amount, string url)
        {
            var paymentLink = new TemporaryPaymentLink(_claimsProvider.GetUserId(), _claimsProvider.GetTenantId(), friendlyOrderId, quickPayPaymentId, amount, url);
            return paymentLink;
        }
    }
}