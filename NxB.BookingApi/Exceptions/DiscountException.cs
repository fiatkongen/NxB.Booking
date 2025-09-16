using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.BookingApi.Exceptions
{
    public class DiscountException : Exception
    {
        public DiscountException()
        {
        }

        public DiscountException(string message)
            : base(message)
        {
        }

        public DiscountException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }
}
