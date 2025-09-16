using System;
using System.Threading.Tasks;
using NxB.Domain.Common.Enums;
using NxB.Dto.AccountingApi;
using QuickPay.SDK;
using QuickPay.SDK.Models.Callbacks;

namespace NxB.BookingApi.Models
{
    public interface IPaymentLinkService
    {
        QuickPayClient CreateQuickPayClient(Guid? tenantId = null);
        Task<PaymentLink> CreatePaymentLink(long friendlyVoucherId, VoucherType voucherType, long friendlyOrderId, decimal amount, PaymentLinkTestMode testMode);
        bool ValidateRequest(string requestBody, string checkSum, Guid tenantId);
        Task<bool> DeletePaymentLink(int quickPayPaymentId, Guid tenantId);
    }
}
