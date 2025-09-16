using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public interface IEqualizeService
    {
        Task<bool> TryEqualizeInvoiceWithSpecificPayment(Payment newPayment, Guid invoiceId, IInvoiceService invoiceService, Guid closeTransactionId);
        Task EqualizeVouchersAndPayments(Payment newPayment, List<Guid> existingPaymentIds, List<Guid> invoiceIds, IInvoiceService invoiceService, Guid closeTransactionId);
        Task VerifyClosedTransactionAmountIsZero(Guid voucherCloseTransactionId);
    }
}
