using NxB.BookingApi.Infrastructure;
using NxB.Domain.Common.Enums;
using NxB.Dto.AccountingApi;
using NxB.Dto.OrderingApi;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public interface IInvoiceService
    {
        Task<Invoice> CreateInvoiceSpecific(Guid id, CreateSpecificInvoiceDto createSpecificInvoiceDto, OrderDto orderDto);
        Task<Invoice> CreateInvoice(Guid id, CreateInvoiceDto createInvoiceDto, OrderDto orderDto);
        Task<Deposit> CreateDeposit(Guid id, CreateDepositDto createDepositDto, OrderDto orderDto, long? friendlyId);
        Task<Deposit> CreateDepositSpecific(Guid id, CreateSpecificDepositDto createSpecificDepositDto, OrderDto orderDto, long? friendlyId);
        Task<CreditNote> Credit(Guid invoiceToCreditId, Guid newCreditedInvoiceId, DateTime voucherDate);
        Task<OrderDto> RemoveInvoicedOrderLines(OrderDto orderDto);
        Task<Invoice> DeleteLegacyInvoice(Guid id);

        Task<Payment> CreatePayment(OrderDto orderDto, decimal amount, PaymentType paymentType,
            string language, DateTime? paymentDate, List<Guid> invoiceIds, List<Guid> existingPaymentIds,
            IEqualizeService equalizeService, DateTime? overrideCreateDate, AppDbContext appDbContext,
            Guid? overridePaymentId,
            Guid? specificInvoiceId = null, long? specificFriendlyInvoiceId = null);

        Task<Payment> CreateOnlinePayment(OrderDto orderDto, decimal amount,
            DueVoucher dueVoucher, IEqualizeService equalizeService, AppDbContext appDbContext);

        Task EqualizeVoucher(Guid orderId, List<Guid> invoiceIds, IEqualizeService equalizeService,
            AppDbContext appDbContext);
    }
}
