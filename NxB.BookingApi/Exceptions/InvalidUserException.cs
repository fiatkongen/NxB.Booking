using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NxB.BookingApi.Exceptions
{
    public class InvalidUserException : Exception
    {
        public InvalidUserException()
        {
        }

        public InvalidUserException(string message)
            : base(message)
        {
        }

        public InvalidUserException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
