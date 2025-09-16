using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.BookingApi.Exceptions
{
    public class PaymentCaptureException : Exception
    {
        public PaymentCaptureException()
        {
        }

        public PaymentCaptureException(string message)
            : base(message)
        {
        }

        public PaymentCaptureException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
