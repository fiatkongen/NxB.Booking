using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.BookingApi.Exceptions
{
    public class RadioException : Exception
    {
        public RadioException()
        {

        }

        public RadioException(string message)
            : base(message)
        {
        }

        public RadioException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}