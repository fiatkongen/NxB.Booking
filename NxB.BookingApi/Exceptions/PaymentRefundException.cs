using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.BookingApi.Exceptions
{
    public class PaymentRefundException : Exception
    {
        public PaymentRefundException()
        {
        }

        public PaymentRefundException(string message)
            : base(message)
        {
        }

        public PaymentRefundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
