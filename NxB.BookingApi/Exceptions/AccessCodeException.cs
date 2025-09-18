using System;

namespace NxB.BookingApi.Exceptions
{
    public class AccessCodeException : Exception
    {
        public AccessCodeException()
        {

        }

        public AccessCodeException(string message)
            : base(message)
        {
        }

        public AccessCodeException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}