using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NxB.Dto.AccountingApi;

namespace NxB.Dto.Clients
{
    public interface IVoucherClient
    {
        Task<InvoiceDto> FindInvoice(Guid id);
        Task<List<PaymentDto>> FindPaymentsFromOrder(Guid orderId);
        Task<List<VoucherDto>> FindPaymentsOrCreditNotesFromOrder(Guid orderId, bool? isClosed);
        Task<List<VoucherDto>> FindVouchersFromAccountId(Guid orderId, bool? isClosed);
        Task<ReadVoucherDto> FindReadVoucherFromDocumentId(Guid documentId);
        Task<Dictionary<Guid, string>> GetInvoicedOrderLineIds(Guid orderId);
        Task<PaymentDto> CreatePayment(CreatePaymentDto createPaymentDto);
        Task<PaymentDto> CreateSpecificPayment(CreateSpecificPaymentDto createPaymentDto);
        Task<DueDepositDto> CreateDeposit(CreateDepositDto createDepositDto);
        Task<List<PaymentDto>> FindSpecificPaymentsFromInvoiceId(Guid invoiceId);
        Task<CreditNoteDto> FindSpecificCreditNoteFromInvoiceId(Guid invoiceId);
        Task<List<CreditNoteDto>> FindSpecificCreditNotesFromVoucherTransactionId(Guid transactionId);
        Task<ReadVoucherDto> CreateReadVoucherDto(CreateReadVoucherDto createReadVoucherDto);
        Task<VoucherDto> FindVoucher(Guid id);
        Task BroadcastDueDepositsCount();
        Task BroadcastDueInvoicesCount();
        Task BroadcastDueVouchersCount();
    }
}
