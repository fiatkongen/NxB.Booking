using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Munk.AspNetCore;
using Munk.Utils.Object;
using NxB.BookingApi.Exceptions;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;
using NxB.Dto.AccountingApi;


namespace NxB.BookingApi.Models
{
    //https://www.e-conomic.dk/regnskabsprogram/ordbog/bilag
    [Serializable]
    public abstract class Voucher : ITenantEntity, IVoucherKey
    {
        public Guid Id { get; internal set; }
        public DateTime CreateDate { get; set; } = DateTime.Now.ToEuTimeZone();
        public Guid CreateAuthorId { get; set; }
        public Guid TenantId { get; internal set; }
        public long FriendlyId { get; set; }
        public Guid AccountId { get; internal set; }
        public string FriendlyAccountId { get; set; }
        public string Text { get; set; }
        public decimal Total { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxTotal { get; set; } 
        public VoucherType VoucherType { get; set; }
        public bool IsClosed { get; protected set; }
        public DateTime? CloseDate { get; protected set; }
        public Guid? VoucherCloseTransactionId { get; set; }
        public string TotalDifferenceText { get; set; }
        public Guid? DocumentId { get; set; }
        public Guid OrderId { get; set; }
        public long FriendlyOrderId { get; set; }
        public string Language { get; set; }
        public Guid? DocumentTemplateId { get; set; }
        public string Note { get; set; }
        public DateTime VoucherDate { get; set; }

        public bool IsOpen => !IsClosed;
        public IAccountKey AccountKey => new AccountKey { Id = AccountId, FriendlyId = FriendlyAccountId };
        public IOrderKey OrderKey => new OrderKey { Id = OrderId, FriendlyId = FriendlyOrderId };

        protected Voucher(Guid id, Guid createAuthorId, Guid tenantId, long friendlyId, Guid accountId, string friendlyAccountId, string text, decimal total, decimal taxTotal, decimal subTotal, Guid orderId, long friendlyOrderId, string language, Guid? documentTemplateId, string note, DateTime voucherDate)
        {
            Id = id;
            CreateAuthorId = createAuthorId;
            TenantId = tenantId;
            FriendlyId = friendlyId;
            AccountId = accountId;
            FriendlyAccountId = friendlyAccountId;
            Text = text;
            Total = total;
            TaxTotal = taxTotal;
            SubTotal = subTotal;
            OrderId = orderId;
            FriendlyOrderId = friendlyOrderId;
            Language = language;
            DocumentTemplateId = documentTemplateId;
            Note = note;
            VoucherDate = voucherDate;
        }

        public virtual void Close(Guid voucherCloseTransactionId)
        {
            this.IsClosed = true;
            this.CloseDate = DateTime.Now.ToEuTimeZone();
            this.VoucherCloseTransactionId = voucherCloseTransactionId;
        }

        public virtual void ResetClose()
        {
            this.IsClosed = false;
            this.CloseDate = null;
            this.VoucherCloseTransactionId = null;
        }
    }

    [Serializable]
    public class DueVoucher : Voucher
    {
        public DateTime DueDate { get; set; }
        public virtual int DueDays => IsClosed ? 0 : DateTime.Now.Date.GetDaysDiff(DueDate);
        public virtual bool IsDue => !IsClosed && DateTime.Now.Date >= DueDate;

        protected DueVoucher(Guid id, Guid createAuthorId, Guid tenantId, long friendlyId, Guid accountId, string friendlyAccountId, string text, decimal total, decimal taxTotal, decimal subTotal, Guid orderId, long friendlyOrderId, string language, Guid? documentTemplateId, string note, DateTime dueDate, DateTime voucherDate) : base(id, createAuthorId, tenantId, friendlyId, accountId, friendlyAccountId, text, total, taxTotal, subTotal, orderId, friendlyOrderId, language, documentTemplateId, note, voucherDate)
        {
            this.DueDate = dueDate;
            this.VoucherType = VoucherType.DueVoucher;
        }
    }

    [Serializable]
    public class DueDeposit : DueVoucher
    {
        public decimal? DepositPercent { get; set; }
        public decimal DepositAmount { get; set; }

        public DueDeposit(Guid id, Guid createAuthorId, Guid tenantId, long friendlyId, Guid accountId, string friendlyAccountId, string text, Guid orderId, long friendlyOrderId, DateTime dueDate, decimal? depositPercent, decimal depositAmount, decimal subTotal, decimal taxTotal, string language, Guid? documentTemplateId, string note, DateTime voucherDate) : base(id, createAuthorId, tenantId, friendlyId, accountId, friendlyAccountId, text, depositAmount, taxTotal, subTotal, orderId, friendlyOrderId, language, documentTemplateId, note, dueDate, voucherDate)
        {
            DepositPercent = depositPercent;
            DepositAmount = depositAmount;

            this.Total = DepositAmount;
            this.SubTotal = subTotal;
            this.VoucherType = VoucherType.DueDeposit;

            var depositText = DepositPercent != null ? DepositPercent.Value.ToDanishDecimalString() + "%" : "";
            TotalDifferenceText = depositText;
        }

        public override void Close(Guid voucherCloseTransactionId)
        {
            this.Close();
        }

        public void Close()
        {
            this.IsClosed = true;
            this.CloseDate = DateTime.Now.ToEuTimeZone();
        }
    }

    [Serializable]
    public abstract class InvoiceBase : DueVoucher
    {
        public List<InvoiceSubOrder> InvoiceSubOrders { get; set; } = new();
        public List<InvoiceLine> InvoiceLines
        {
            get { return InvoiceSubOrders.SelectMany(x => x.InvoiceLines).ToList(); }
        }
        public List<InvoiceOrderLine> InvoiceOrderLines => InvoiceLines.OfType<InvoiceOrderLine>().ToList();

        protected InvoiceBase(Guid id, Guid createAuthorId, Guid tenantId, long friendlyId, Guid accountId, string friendlyAccountId, string text, decimal total, decimal taxTotal, decimal subTotal, Guid orderId, long friendlyOrderId, string language, Guid? documentTemplateId, string note, DateTime dueDate, DateTime voucherDate) : base(id, createAuthorId, tenantId, friendlyId, accountId, friendlyAccountId, text, total, taxTotal, subTotal, orderId, friendlyOrderId, language, documentTemplateId, note, dueDate, voucherDate)
        {
        }

        public void CreateInvoiceSubOrder(Guid id, Guid subOrderId, int index, DateTime? start, DateTime? end, string rentalUnitName)
        {
            if (id == Guid.Empty) throw new VoucherException(nameof(id) + " is empty");
            if (subOrderId == Guid.Empty) throw new VoucherException(nameof(subOrderId) + " is empty");
            if (InvoiceSubOrders.Any(x => x.Id == id)) throw new VoucherException($"InvoiceSubOrder with id {id} has already been added");
            rentalUnitName = rentalUnitName != null && rentalUnitName.Length > 100 ? rentalUnitName.Substring(0, 100) : rentalUnitName;

            var invoiceSubOrder = new InvoiceSubOrder
            {
                Id = id,
                Voucher = this,
                VoucherId = this.Id,
                Index = index,
                SubOrderId = subOrderId,
                Start = start,
                End = end,
                RentalUnitName = rentalUnitName
            };
            InvoiceSubOrders.Add(invoiceSubOrder);
        }

        public void RemoveInvoiceSubOrder(Guid id)
        {
            this.InvoiceSubOrders.RemoveAll(x => x.Id == id);
        }

        public void CreateInvoiceOrderLine(Guid id, Guid invoiceSubOrderId, Guid orderLineId, decimal index, decimal number, string text, DateTime? start, DateTime? end, Guid priceProfileId, string priceProfileName, decimal? tax, decimal? taxPercent, decimal pricePcs, bool isDiscount)
        {
            if (id == Guid.Empty) throw new VoucherException(nameof(id) + " is empty");
            if (invoiceSubOrderId == Guid.Empty) throw new VoucherException(nameof(invoiceSubOrderId) + " is empty");
            if (InvoiceLines.Any(x => x.Id == id)) throw new VoucherException($"InvoiceOrderLine with id {id} has already been added");

            var invoiceSubOrder = InvoiceSubOrders.Single(x => x.Id == invoiceSubOrderId);
            if (invoiceSubOrder == null) throw new VoucherException($"InvoiceSubOrder with id {id} has not been added. InvoiceSubOrderLine cannot be added to this SubOrder");

            var invoiceOrderLine = new InvoiceOrderLine
            {
                Id = id,
                TenantId = this.TenantId,
                OrderLineId = orderLineId,
                InvoiceSubOrder = invoiceSubOrder,
                InvoiceSubOrderId = invoiceSubOrder.Id,
                Index = index,
                Number = number,
                Text = text,
                Start = start,
                End = end,
                PriceProfileId = priceProfileId,
                PriceProfileName = priceProfileName,
                Tax = tax,
                TaxPercent = taxPercent,
                PricePcs = pricePcs,
                Total = Math.Round(pricePcs * number, 2, MidpointRounding.AwayFromZero),
                IsDiscount = isDiscount
            };
            invoiceSubOrder.InvoiceLines.Add(invoiceOrderLine);
        }

        public virtual void CalculateTotals()
        {
            Total = InvoiceOrderLines.Sum(x => x.Total);
            SubTotal = Total;
        }

        public void CreateInvoiceTextLine(Guid id, Guid invoiceSubOrderId, decimal index, decimal number, string text, decimal pricePcs)
        {
            if (id == Guid.Empty) throw new VoucherException(nameof(id) + " is empty");
            if (invoiceSubOrderId == Guid.Empty) throw new VoucherException(nameof(invoiceSubOrderId) + " is empty");
            if (InvoiceLines.Any(x => x.Id == id)) throw new VoucherException($"InvoiceOrderLine with id {id} has already been added");

            var invoiceSubOrder = InvoiceSubOrders.Single(x => x.Id == invoiceSubOrderId);
            if (invoiceSubOrder == null) throw new VoucherException($"InvoiceSubOrder with id {id} has not been added. InvoiceSubOrderLine cannot be added to this SubOrder");

            var invoiceTextLine = new InvoiceTextLine
            {
                Id = id,
                InvoiceSubOrder = invoiceSubOrder,
                InvoiceSubOrderId = invoiceSubOrder.Id,
                Index = index,
                Number = number,
                Text = text,
                PricePcs = pricePcs,
                Total = Math.Round(pricePcs * number, 2, MidpointRounding.AwayFromZero),
            };
            invoiceSubOrder.InvoiceLines.Add(invoiceTextLine);
        }

    }

    [Serializable]
    public class Invoice : InvoiceBase
    {
        public Invoice(Guid id, Guid createAuthorId, Guid tenantId, long friendlyId, Guid accountId, string friendlyAccountId, string text, Guid orderId, long friendlyOrderId, DateTime dueDate, string language, Guid? documentTemplateId, string note, DateTime voucherDate) : base(id, createAuthorId, tenantId, friendlyId, accountId, friendlyAccountId, text, 0, 0, 0, orderId, friendlyOrderId, language, documentTemplateId, note, dueDate, voucherDate)
        {
            this.VoucherType = VoucherType.Invoice;
        }
    }

    [Serializable]
    public class Deposit : Invoice
    {
        public decimal? DepositPercent { get; set; }
        public decimal DepositAmount { get; set; }

        public Deposit(Guid id, Guid createAuthorId, Guid tenantId, long friendlyId, Guid accountId, string friendlyAccountId, string text, Guid orderId, long friendlyOrderId, DateTime dueDate, decimal? depositPercent, decimal depositAmount, string language, Guid? documentTemplateId, string note, DateTime voucherDate) : base(id, createAuthorId, tenantId, friendlyId, accountId, friendlyAccountId, text, orderId, friendlyOrderId, dueDate, language, documentTemplateId, note, voucherDate)
        {
            DepositPercent = depositPercent;
            DepositAmount = depositAmount;
            this.VoucherType = VoucherType.Deposit;
        }

        public override void CalculateTotals()
        {
            SubTotal = InvoiceOrderLines.Sum(x => x.Total);
            Total = DepositAmount;
            var depositText = DepositPercent != null ? DepositPercent.Value.ToDanishDecimalString() + "%" : "";
            TotalDifferenceText = depositText;
        }
    }

    [Serializable]
    public class CreditNote : InvoiceBase
    {
        public override int DueDays => 0;
        public override bool IsDue => false;
        public Guid InvoiceId { get; set; }
        public long FriendlyInvoiceId { get; set; }

        public CreditNote(Guid id, Guid createAuthorId, Guid tenantId, long friendlyId, Guid accountId, string friendlyAccountId, string text, decimal total, decimal taxTotal, decimal subTotal, Guid invoiceId, long friendlyInvoiceId, Guid orderId, long friendlyOrderId, string language, Guid? documentTemplateId, string note, DateTime voucherDate) : base(id, createAuthorId, tenantId, friendlyId, accountId, friendlyAccountId, text, total, taxTotal, subTotal, orderId, friendlyOrderId, language, documentTemplateId, note, voucherDate, voucherDate)
        {
            this.VoucherType = VoucherType.CreditNote;
            InvoiceId = invoiceId;
            FriendlyInvoiceId = friendlyInvoiceId;
        }
    }

    //indbetalinger http://wiki2.e-conomic.dk/regnskab/kassekladder-leverandoerbetaling
    [Serializable]
    public class Payment : Voucher
    {
        public PaymentType PaymentType { get; set; }
        public Guid? SpecificInvoiceId { get; set; }
        public long? SpecificFriendlyInvoiceId { get; set; }

        public Payment(Guid id, Guid createAuthorId, Guid tenantId, long friendlyId, Guid accountId, string friendlyAccountId, decimal total, decimal taxTotal, decimal subTotal, PaymentType paymentType, Guid orderId, long friendlyOrderId, string language, Guid? specificInvoiceId, long? specificFriendlyInvoiceId, Guid? documentTemplateId, DateTime voucherDate) : base(id, createAuthorId, tenantId, friendlyId, accountId, friendlyAccountId, null, total, taxTotal, subTotal, orderId, friendlyOrderId, language, documentTemplateId, null, voucherDate)
        {
            this.VoucherType = VoucherType.Payment;
            this.PaymentType = paymentType;
            SpecificInvoiceId = specificInvoiceId; // this property should not be used, but is kept for legacy: Not sure what kind of problems would arise if removed
            SpecificFriendlyInvoiceId = specificFriendlyInvoiceId;
            UpdateText();
        }

        public void UpdateText()
        {
            switch (this.PaymentType)
            {
                case PaymentType.None:
                    throw new ArgumentException("Paymenttype cannot be None");
                case PaymentType.Cash:
                    this.Text = "Indbetaling, kontant";
                    break;
                case PaymentType.BankTransfer:
                    this.Text = "Indbetaling, bankoverførsel";
                    break;
                case PaymentType.OnAccount:
                    this.Text = "Indbetaling, aconto";
                    break;
                case PaymentType.Online:
                    this.Text = "Indbetaling, online";
                    break;
                case PaymentType.MobilePay:
                    this.Text = "Indbetaling, mobilepay";
                    break;
                case PaymentType.CreditVoucher:
                    this.Text = "Indbetaling, tilgodebevis";
                    break;
                case PaymentType.CreditCard:
                    this.Text = "Indbetaling, kreditkort";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (this.SpecificFriendlyInvoiceId.HasValue)
            {
                this.Text += $" (til faktura {SpecificFriendlyInvoiceId.Value.DefaultIdPadding()})";
            }
        }

        public void SetSpecificInvoice(InvoiceBase invoiceBase)
        {
            this.SpecificInvoiceId = invoiceBase.Id;
            this.SpecificFriendlyInvoiceId = invoiceBase.FriendlyId;
            this.UpdateText();
        }

        public Payment Revert(IFriendlyAccountingIdProvider friendlyIdProvider)
        {
            var payment = new Payment(Guid.NewGuid(), this.CreateAuthorId, this.TenantId, friendlyIdProvider.GenerateNextFriendlyPaymentId(), this.AccountId, this.FriendlyAccountId, 0 - this.Total, 0 - this.TaxTotal, 0 - this.SubTotal, this.PaymentType, this.OrderId, this.FriendlyOrderId, this.Language, null, SpecificFriendlyInvoiceId, this.DocumentTemplateId, DateTime.Today);
            return payment;
        }

        public void ForceOpenHack()
        {
            this.IsClosed = false;
            this.VoucherCloseTransactionId = null;
            this.CloseDate = null;
        }
    }
}


