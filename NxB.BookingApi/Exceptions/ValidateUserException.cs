using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NxB.BookingApi.Exceptions
{
    public class ValidateUserException : Exception
    {
        public ValidateUserException()
        {
        }

        public ValidateUserException(string message)
            : base(message)
        {
        }

        public ValidateUserException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
