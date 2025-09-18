using System;

namespace NxB.BookingApi.Models
{
    // TODO: Implement RadioBillingFactory - placeholder to fix compilation errors
    // This factory should create RadioBilling domain objects
    public class RadioBillingFactory
    {
        // TODO: Implement factory methods
        public RadioBilling Create(int radioAddress, decimal amount, string description)
        {
            return new RadioBilling(Guid.NewGuid(), radioAddress, amount, DateTime.UtcNow, description);
        }

        public RadioBilling CreateFromEntity(object entity)
        {
            // TODO: Implement entity conversion logic
            throw new NotImplementedException("RadioBillingFactory.CreateFromEntity needs implementation");
        }
    }
}