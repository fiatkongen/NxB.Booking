using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class InvoiceSubOrder
    {
        public Guid Id { get; set; }
        public Guid SubOrderId { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string RentalUnitName { get; set; }

        public Voucher Voucher { get; set; }
        public Guid VoucherId { get; set; }
        public int Index { get; set; }

        public List<InvoiceLine> InvoiceLines { get; set; } = new();
    }


    [Serializable]
    public class InvoiceLine
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; internal set; }
        public decimal Number { get; set; }
        public decimal PricePcs { get; set; }
        public decimal Total { get; set; }
        public decimal? DepositTotal { get; set; }  //not in use right now, saved for future use
        public string Text { get; set; }
        public decimal Index { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        public Guid InvoiceSubOrderId { get; set; }
        public InvoiceSubOrder InvoiceSubOrder { get; set; }
    }

    [Serializable]
    public class InvoiceTextLine : InvoiceLine
    {
    }

    [Serializable]
    public class InvoiceOrderLine : InvoiceLine
    {
        public Guid OrderLineId { get; set; }
        public decimal? Tax { get; set; }
        public decimal? TaxPercent { get; set; }
        public string PriceProfileName { get; set; }
        public Guid PriceProfileId { get; set; }
        public bool IsDiscount { get; set; }
    }
}