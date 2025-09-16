using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Domain.Common.Model
{
    public class InvoicedOrderLineInfo
    {
        public Guid OrderLineId { get; set; }
        public Guid InvoiceId { get; set; }
        public long InvoiceFriendlyId { get; set; }
        public Guid DocumentId { get; set; }
        public string Color { get; set; }

        public InvoicedOrderLineInfo(Guid orderLineId, Guid invoiceId, long invoiceFriendlyId, Guid documentId, string color)
        {
            OrderLineId = orderLineId;
            InvoiceId = invoiceId;
            InvoiceFriendlyId = invoiceFriendlyId;
            DocumentId = documentId;
            Color = color;
        }
    }
}
