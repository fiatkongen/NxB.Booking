using System;

namespace NxB.BookingApi.Exceptions
{
    public class SocketException : Exception
    {
        public SocketException()
        {

        }

        public SocketException(string message)
            : base(message)
        {
        }

        public SocketException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}