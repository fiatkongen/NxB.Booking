using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Dto.AccountingApi
{
    public class AccountTotalsDto
    {
        public Guid AccountId { get; set; }
        public Guid? OrderId { get; set; }
        public decimal Invoices { get; set; }
        public int InvoicesCount { get; set; }
        public decimal DueInvoices { get; set; }
        public int DueInvoicesCount { get; set; }
        public decimal Payments { get; set; }
        public int PaymentsCount { get; set; }
        public decimal NotInvoiced { get; set; }

        public decimal? EurConversionRate { get; set; } = null;

        public decimal GetAmountDue()
        {
            return Invoices + NotInvoiced + DueInvoices + Payments;
        }

    }
}

