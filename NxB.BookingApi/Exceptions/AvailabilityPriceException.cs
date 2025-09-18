using System;

namespace NxB.BookingApi.Exceptions
{
    public class AvailabilityPriceException : Exception
    {
        public AvailabilityPriceException() : base()
        {
        }

        public AvailabilityPriceException(string message) : base(message)
        {
        }

        public AvailabilityPriceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}