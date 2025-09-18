using System;

namespace NxB.BookingApi.Exceptions
{
    public class RadioAccessException : Exception
    {
        public RadioAccessException()
        {

        }

        public RadioAccessException(string message)
            : base(message)
        {
        }

        public RadioAccessException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}