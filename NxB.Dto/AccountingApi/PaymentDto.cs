using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Munk.Utils.Object;
using NxB.Domain.Common.Enums;

namespace NxB.Dto.AccountingApi
{
    public class PaymentDto : VoucherDto
    {
        public PaymentType PaymentType { get; set; }
        public Guid? SpecificInvoiceId { get; set; }
        public long? SpecificFriendlyInvoiceId { get; set; }
    }

    public class BasePaymentDto
    {
        [Required]
        [NoEmpty]
        public Guid OrderId { get; set; }

        public decimal PaymentAmount { get; set; }
        public PaymentType PaymentType { get; set; }

        public bool SkipDocumentCreate { get; set; } = true;
        public string Language { get; set; } = "da";
        public DateTime? PaymentDate { get; set; }

        public Guid? SaveId { get; set; }
    }

    public class EqualizePaymentsDto : BasePaymentDto
    {
        public List<Guid> InvoiceIds { get; set; } = new();
        public List<Guid> ExistingPaymentIds { get; set; } = new();
    }


    public class CreatePaymentDto : EqualizePaymentsDto
    {
        public DateTime? OverrideCreateDate { get; set; }
        public Guid? OverridePaymentId { get; set; }
    }

    public class CreateSpecificPaymentDto : BasePaymentDto
    {
        [NoEmpty]
        public Guid SpecificInvoiceId { get; set; }
        public long SpecificFriendlyInvoiceId { get; set; }
    }
}