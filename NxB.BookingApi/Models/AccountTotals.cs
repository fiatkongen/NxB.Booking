using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.BookingApi.Models
{
    public class AccountTotals
    {
        public decimal Invoices { get; set; }
        public int InvoicesCount { get; set; }
        public decimal DueInvoices { get; set; }
        public int DueInvoicesCount { get; set; }
        public int PaymentsCount { get; set; }
        public decimal Payments { get; set; }
        public decimal Balance => Invoices - Payments;
        public decimal NotInvoiced { get; set; }
    }
}
