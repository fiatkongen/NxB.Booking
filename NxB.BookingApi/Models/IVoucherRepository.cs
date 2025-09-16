using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Model;
using NxB.Dto.AccountingApi;
using NxB.Dto.OrderingApi;

namespace NxB.BookingApi.Models
{
    public interface IVoucherRepository
    {
        void Add(Voucher invoice);
        void Update(Voucher invoice);
        void DeleteLegacyInvoice(Voucher invoice);
        Payment FindSinglePayment(Guid id);
        TInvoiceType FindSingleInvoiceBase<TInvoiceType>(Guid id) where TInvoiceType : InvoiceBase;
        TVoucherType FindSingleVoucher<TVoucherType>(Guid id) where TVoucherType : Voucher;
        TInvoiceType FindSingleInvoiceFromFriendlyId<TInvoiceType>(long friendlyId) where TInvoiceType : InvoiceBase;
        TVoucherType FindSingleVoucherFromFriendlyId<TVoucherType>(long friendlyId, VoucherType voucherType) where TVoucherType : Voucher; //The type is neccessary since payments and invoices can have overlapping friendly Ids
        TVoucherType FindSingleOrDefaultVoucherFromFriendlyId<TVoucherType>(long friendlyId, VoucherType voucherType) where TVoucherType : Voucher;
        Task<List<TVoucher>> FindAll<TVoucher>(DateInterval dateInterval, bool? isClosed = null) where TVoucher : Voucher;
        Task<List<TDueVoucher>> FindAllDueVouchers<TDueVoucher>(DateInterval dateInterval, bool? isClosed = null, bool? isDue = null) where TDueVoucher : DueVoucher;
        Task<TVoucher> FindSingleFromDocumentId<TVoucher>(Guid documentId) where TVoucher : Voucher;
        Task<List<TInvoiceType>> FindInvoiceBases<TInvoiceType>(DateInterval dateInterval, bool ignoreSubItems, bool? isClosed, bool? isDue) where TInvoiceType : InvoiceBase;
        Task<List<TInvoiceType>> FindInvoiceBasesFromIds<TInvoiceType>(List<Guid> invoiceIds, bool? isClosed) where TInvoiceType : InvoiceBase;
        Task<List<TVoucherType>> FindVouchersFromIds<TVoucherType>(List<Guid> invoiceIds, bool? isClosed) where TVoucherType : Voucher;
        Task<List<TInvoiceType>> FindInvoiceBasesFromOrderId<TInvoiceType>(Guid orderId, bool ignoreSubItems) where TInvoiceType : InvoiceBase;
        Task<List<TInvoiceType>> FindInvoiceBasesFromFriendlyOrderId<TInvoiceType>(long friendlyOrderId, bool ignoreSubItems) where TInvoiceType : InvoiceBase;
        Task<List<Payment>> FindPaymentsFromOrderId(Guid orderId);
        Task<List<Payment>> FindPaymentsFromFriendlyOrderId(long friendlyOrderId);
        Task<List<Voucher>> FindPaymentsOrCreditNotesFromOrder(Guid orderId, bool? isClosed = null);
        Task<List<Payment>> FindSpecificPaymentsFromInvoiceId(Guid invoiceId, bool? isClosed);
        Task<List<Payment>> FindPaymentsFromClosedTransactionId(Guid closeTransactionId);
        Task<List<Payment>> FindPaymentsFromIds(List<Guid> paymentIds, bool? isClosed);
        Task<List<TVoucher>> FindFromAccountId<TVoucher>(Guid accountId, bool? isClosed) where TVoucher : Voucher;
        Task<List<TInvoiceType>> FindFromOrderId<TInvoiceType>(Guid orderId, bool? isClosed) where TInvoiceType : Voucher;
        Task<List<TInvoiceType>> FindFromFriendlyOrderId<TInvoiceType>(long orderId, bool? isClosed) where TInvoiceType : Voucher;
        Task<List<Guid>> RemoveInvoicedOrderLineIds(List<Guid> orderLineIds);
        Task<Dictionary<Guid, string>> GetInvoicedOrderLineIds(Guid orderId);
        Task<List<InvoicedOrderLineInfo>> GetInvoicedOrderLinesInfo(Guid orderId);
        CreditNote FindSingleOrDefaultCreditNoteFromInvoiceId(Guid invoiceId);
        Task<List<Voucher>> FindVouchersFromClosedTransactionId(Guid closeTransactionId);
        Task<decimal> CalculateTotalFromOrderId(Guid orderId);
        Task<decimal> CalculateTotalFromAccountId(Guid orderId);
    }
}
