using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NxB.BookingApi.Exceptions
{
    public class ChangePasswordException : Exception
    {
        public ChangePasswordException()
        {
        }

        public ChangePasswordException(string message)
            : base(message)
        {
        }

        public ChangePasswordException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
