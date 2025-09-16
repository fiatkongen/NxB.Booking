using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.BookingApi.Exceptions
{
    public class PaymentCompletionException : Exception
    {
        public PaymentCompletionException()
        {
        }

        public PaymentCompletionException(string message)
            : base(message)
        {
        }

        public PaymentCompletionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
