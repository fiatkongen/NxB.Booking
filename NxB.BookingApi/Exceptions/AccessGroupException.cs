using System;

namespace NxB.BookingApi.Exceptions
{
    public class AccessGroupException : Exception
    {
        public AccessGroupException()
        {

        }

        public AccessGroupException(string message)
            : base(message)
        {
        }

        public AccessGroupException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}