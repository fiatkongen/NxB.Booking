using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NxB.BookingApi.Exceptions
{
    public class StartUpException : Exception
    {
        public StartUpException() : base()
        {
        }

        public StartUpException(string message) : base(message)
        {
        }

        public StartUpException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}