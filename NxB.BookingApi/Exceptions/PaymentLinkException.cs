using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.BookingApi.Exceptions
{
    public class PaymentLinkException : Exception
    {
        public PaymentLinkException()
        {
        }

        public PaymentLinkException(string message)
            : base(message)
        {
        }

        public PaymentLinkException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
