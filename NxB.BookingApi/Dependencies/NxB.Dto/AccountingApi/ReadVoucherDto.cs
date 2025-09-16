using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using NxB.Domain.Common.Enums;

namespace NxB.Dto.AccountingApi
{
    public class ReadVoucherDto : VoucherDto
    {
        public Guid TenantId { get; set; }

        //From InvoiceBaseDto
        public bool? IsCredited { get; set; }

        //From InvoiceDto
        public DateTime? DueDate { get; set; }
        public int? DueDays { get; set; }
        public bool? IsDue { get; set; }

        //CreditNoteDto
        public Guid? InvoiceId { get; set; }
        public long? FriendlyInvoiceId { get; set; }

        //From PaymentDTo
        public PaymentType PaymentType { get; set; }

        public Guid? SpecificInvoiceId { get; set; }
        public long? SpecificFriendlyInvoiceId { get; set; }

        public List<InvoiceSubOrderDto> InvoiceSubOrders { get; set; }
    }
}
