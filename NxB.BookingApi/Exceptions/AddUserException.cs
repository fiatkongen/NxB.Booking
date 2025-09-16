using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NxB.BookingApi.Exceptions
{
    public class AddUserException : Exception
    {
        public AddUserException()
        {
        }

        public AddUserException(string message)
            : base(message)
        {
        }

        public AddUserException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
