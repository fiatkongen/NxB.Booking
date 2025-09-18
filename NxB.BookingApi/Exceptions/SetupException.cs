using System;

namespace NxB.BookingApi.Exceptions
{
    public class SetupException : Exception
    {
        public SetupException()
        {

        }

        public SetupException(string message)
            : base(message)
        {
        }

        public SetupException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}