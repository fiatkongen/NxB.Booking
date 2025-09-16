using System;
using System.Collections.Generic;
using System.Text;
using Munk.AspNetCore;

namespace NxB.BookingApi.Exceptions
{
    public class AddSubOrdersException : Exception
    {
        public AddSubOrdersException()
        {
        }

        public AddSubOrdersException(string message)
            : base(message)
        {
        }

        public AddSubOrdersException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
