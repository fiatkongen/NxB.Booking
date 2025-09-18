using System;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    // TODO: Implement RadioBilling domain model - placeholder to fix compilation errors
    // This class represents radio billing information
    public class RadioBilling : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public int RadioAddress { get; set; }
        public decimal Amount { get; set; }
        public DateTime BillingDate { get; set; }
        public string Description { get; set; }

        // TODO: Implement domain logic methods
        public RadioBilling()
        {
        }

        public RadioBilling(Guid id, int radioAddress, decimal amount, DateTime billingDate, string description)
        {
            Id = id;
            RadioAddress = radioAddress;
            Amount = amount;
            BillingDate = billingDate;
            Description = description;
        }
    }
}