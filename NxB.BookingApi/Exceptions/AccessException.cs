using System;
using Munk.AspNetCore;

namespace NxB.BookingApi.Exceptions
{
    public class AccessException : Exception
    {
        public AccessException()
        {

        }

        public AccessException(string message)
            : base(message)
        {
        }

        public AccessException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
