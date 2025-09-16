using System;

namespace NxB.BookingApi.Exceptions
{
    public class ValidateOrderException : Exception
    {
        public ValidateOrderException()
        {
        }

        public ValidateOrderException(string message)
            : base(message)
        {
        }

        public ValidateOrderException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
