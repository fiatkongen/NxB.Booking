using System;
using Munk.AspNetCore;
using Munk.Utils.Object;
using NxB.BookingApi.Infrastructure;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;
using NxB.Dto.AccountingApi;
using NxB.Dto.OrderingApi;

namespace NxB.BookingApi.Models
{
    public class VoucherFactory
    {
        private readonly IClaimsProvider _claimsProvider;
        private readonly IFriendlyAccountingIdProvider _friendlyIdProvider;

        public VoucherFactory(IFriendlyAccountingIdProvider friendlyIdProvider, IClaimsProvider claimsProvider)
        {
            _friendlyIdProvider = friendlyIdProvider;
            _claimsProvider = claimsProvider;
        }

        public Invoice CreateInvoice(Guid id, IAccountKey accountKey, IOrderKey orderKey, DateTime dueDate, string language, Guid? documentTemplateId, string note, DateTime voucherDate)
        {
            var friendlyInvoiceId = _friendlyIdProvider.GenerateNextFriendlyInvoiceId();
            var invoiceText = "Faktura for booking " + orderKey.FriendlyId.DefaultIdPadding();

            var invoice = new Invoice(id, _claimsProvider.GetUserId(), _claimsProvider.GetTenantId(), friendlyInvoiceId, accountKey.Id, accountKey.FriendlyId, invoiceText, orderKey.Id, orderKey.FriendlyId, dueDate, language, documentTemplateId, note, voucherDate);

            return invoice;
        }

        public Deposit CreateDeposit(Guid id, IAccountKey accountKey, IOrderKey orderKey, DateTime dueDate, decimal? depositPercent, decimal depositAmount, string language, Guid? documentTemplateId, string note, DateTime voucherDate, long? friendlyId)
        {
            friendlyId = friendlyId.HasValue ? friendlyId.Value : _friendlyIdProvider.GenerateNextFriendlyInvoiceId();
            var invoiceText = "Depositum for booking " + orderKey.FriendlyId.DefaultIdPadding();

            var deposit = new Deposit(id, _claimsProvider.GetUserId(), _claimsProvider.GetTenantId(), friendlyId.Value, accountKey.Id, accountKey.FriendlyId, invoiceText, orderKey.Id, orderKey.FriendlyId, dueDate, depositPercent, depositAmount, language, documentTemplateId, note, voucherDate);

            return deposit;
        }

        public DueDeposit CreateDueDeposit(Guid id, long friendlyInvoiceId, IAccountKey accountKey, IOrderKey orderKey, DateTime dueDate, decimal? depositPercent, decimal depositAmount, decimal fullAmount, string language, Guid? documentTemplateId, string note, DateTime voucherDate)
        {
            var dueDeposit = new DueDeposit(id, _claimsProvider.GetUserId(), _claimsProvider.GetTenantId(), friendlyInvoiceId, accountKey.Id, accountKey.FriendlyId, "Bekræftelse", orderKey.Id, orderKey.FriendlyId, dueDate, depositPercent, depositAmount, fullAmount, 0, language, documentTemplateId, note, voucherDate);

            return dueDeposit;
        }

        public CreditNote CreateCreditNote(Guid id, IAccountKey accountKey, IVoucherKey voucherKey, IOrderKey orderKey, string language, Guid? documentTemplateId, DateTime voucherDate)
        {
            var friendlyCreditNoteId = _friendlyIdProvider.GenerateNextFriendlyInvoiceId();

            var invoiceText = "Kreditnota for faktura " + voucherKey.FriendlyId.DefaultIdPadding();
            var creditNote = new CreditNote(id, _claimsProvider.GetUserId(), _claimsProvider.GetTenantId(), friendlyCreditNoteId, accountKey.Id, accountKey.FriendlyId, invoiceText, 0, 0, 0, voucherKey.Id, voucherKey.FriendlyId, orderKey.Id, orderKey.FriendlyId, language, documentTemplateId, null, voucherDate);
            return creditNote;
        }

        public Payment CreatePayment(Guid id, IAccountKey accountKey, decimal amount, PaymentType paymentType, IOrderKey orderKey, string language, DateTime? paymentDate, Guid? documentTemplateId, Guid? specificInvoiceId, long? specificFriendlyInvoiceId)
        {
            var friendlyPaymentId = _friendlyIdProvider.GenerateNextFriendlyPaymentId();
            var total = 0 - amount;
            var payment = new Payment(id, _claimsProvider.GetUserId(), _claimsProvider.GetTenantId(), friendlyPaymentId, accountKey.Id, accountKey.FriendlyId, total, total * 0.25m, total, paymentType, orderKey.Id, orderKey.FriendlyId, language, specificInvoiceId, specificFriendlyInvoiceId, documentTemplateId, DateTime.Now.ToEuTimeZone().Date);
            if (paymentDate != null)
            {
                payment.VoucherDate = paymentDate.Value;
            }
            payment.DocumentId = Guid.NewGuid();
            return payment;
        }

        public Payment CreateSpecificPayment(Guid id, IAccountKey accountKey, decimal amount, PaymentType paymentType, IOrderKey orderKey, Guid invoiceId, long friendlyInvoiceId, string language, DateTime? paymentDate, Guid? documentTemplateId)
        {
            var friendlyPaymentId = _friendlyIdProvider.GenerateNextFriendlyPaymentId();
            var total = 0 - amount;
            var payment = new Payment(id, _claimsProvider.GetUserId(), _claimsProvider.GetTenantId(), friendlyPaymentId, accountKey.Id, accountKey.FriendlyId, total, total * 0.25m, total, paymentType, orderKey.Id, orderKey.FriendlyId, language, invoiceId, friendlyInvoiceId, documentTemplateId, DateTime.Now.ToEuTimeZone().Date);
            if (paymentDate != null)
            {
                payment.VoucherDate = paymentDate.Value;
            }
            payment.DocumentId = Guid.NewGuid();
            return payment;
        }

    }
}
