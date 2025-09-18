using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Text;
using Munk.Utils.Object;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.OrderingApi;

namespace NxB.Dto.AccountingApi
{
    public class VoucherDto : IVoucherKey
    {
        [Required]
        [NoEmpty]
        public Guid Id { get; set; }

        public long FriendlyId { get; set; }
        public DateTime CreateDate { get; set; }

        [Required]
        [NoEmpty]
        public Guid CreateAuthorId { get; set; }
        public string CreateAuthorName { get; set; }

        [Required]
        [NoEmpty]
        public Guid AccountId { get; set; }
        public string FriendlyAccountId { get; set; }

        public string Text { get; set; }
        public decimal Total { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxTotal { get; set; }

        public VoucherType VoucherType { get; set; }
        public bool IsClosed { get; set; }
        public DateTime? CloseDate { get; set; }
        public Guid? VoucherCloseTransactionId { get; set; }
        public string TotalDifferenceText { get; set; }

        public Guid? DocumentId { get; set; }
        public Guid OrderId { get; set; }
        public long FriendlyOrderId { get; set; }
        public string Language { get; set; }
        public Guid? DocumentTemplateId { get; set; }
        public string Note { get; set; }
        public DateTime VoucherDate { get; set; }
    }

    public class InvoiceBaseDto : VoucherDto
    {
        public bool IsCredited { get; set; }

        public DateTime DueDate { get; set; }
        public int DueDays { get; set; }
        public bool IsDue { get; set; }

        public List<InvoiceSubOrderDto> InvoiceSubOrders { get; set; } = new();
    }

    public class InvoiceDto : InvoiceBaseDto
    {
    }

    public class DueDepositDto : VoucherDto
    {
        public decimal? DepositPercent { get; set; }
        public decimal DepositAmount { get; set; }
        public DateTime DueDate { get; set; }
        public int DueDays { get; set; }
        public bool IsDue { get; set; }
    }

    public class DepositDto : InvoiceDto
    {
        public decimal? DepositPercent { get; set; }
        public decimal? DepositAmount { get; set; }
    }

    public class CreditNoteDto : InvoiceBaseDto
    {
        [Required]
        [NoEmpty]
        public Guid InvoiceId { get; set; }
        public long FriendlyInvoiceId { get; set; }
    }

    public class InvoiceSubOrderDto
    {
        public Guid Id { get; set; }
        public int Index { get; set; }
        public Guid SubOrderId { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string RentalUnitName { get; set; }

        public List<InvoiceLineDto> InvoiceLines { get; set; }
        public List<InvoiceLineDto> InvoiceOrderLines => InvoiceLines.Where(x => x.Type == InvoiceLineDtoType.OrderLine).ToList();
    }

    public class InvoiceLineDto
    {
        public Guid Id { get; set; }
        public decimal Number { get; set; }
        public decimal PricePcs { get; set; }
        public decimal Total { get; set; }
        public string Text { get; set; }
        public decimal Index { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public InvoiceLineDtoType Type { get; set; }
        public Guid? OrderLineId { get; set; }
        public decimal? Tax { get; set; }
        public decimal? TaxPercent { get; set; }
    }

    public class CreateInvoiceDto : CreatePaymentDto
    {
        [Required]
        [NoEmpty]
        public Guid AccountId { get; set; }

        [Required]
        [NoEmpty]
        public Guid? InvoiceTemplateId { get; set; }

        public DateTime DueDate { get; set; }
        public DateTime VoucherDate { get; set; } = DateTime.Now.ToEuTimeZone().Date;

        public string Note { get; set; }
        public string OverrideDocumentText { get; set; }
    }

    public class CreateInvoiceSubOrderDto
    {
        public Guid SubOrderId { get; set; }
        public List<Guid> InvoiceOrderLinesIds { get; set; }
    }


    public class CreateSpecificInvoiceDto : CreateInvoiceDto, ICreateInvoiceSubOrder
    {
        public List<CreateInvoiceSubOrderDto> CreateInvoiceSubOrders { get; set; }
    }

    public class CreateDepositDto : CreateInvoiceDto
    {
        public decimal? DepositPercent { get; set; }
        public decimal DepositAmount { get; set; }
    }

    public class CreateSpecificDepositDto : CreateDepositDto, ICreateInvoiceSubOrder
    {
        public List<CreateInvoiceSubOrderDto> CreateInvoiceSubOrders { get; set; }
    }

    public interface ICreateInvoiceSubOrder
    {
        List<CreateInvoiceSubOrderDto> CreateInvoiceSubOrders { get; }
        string Language { get; set; }
    }

    public enum InvoiceLineDtoType
    {
        None,
        Text,
        OrderLine
    }

    public class CreateReadVoucherDto
    {
        public long FriendlyVoucherId { get; set; }
        public CreateDepositDto CreateDepositDto { get; set; }
        public OrderDto OrderDto { get; set; }
    }
}
