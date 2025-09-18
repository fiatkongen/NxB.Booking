using System;

namespace NxB.BookingApi.Exceptions
{
    public class PriceCalculationException : Exception
    {
        public PriceCalculationException()
        {

        }

        public PriceCalculationException(string message)
            : base(message)
        {
        }

        public PriceCalculationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}