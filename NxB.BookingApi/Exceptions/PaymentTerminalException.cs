using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NxB.BookingApi.Exceptions
{
    public class PaymentTerminalException : Exception
    {
        public PaymentTerminalException() : base()
        {
        }

        public PaymentTerminalException(string message) : base(message)
        {
        }

        public PaymentTerminalException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}