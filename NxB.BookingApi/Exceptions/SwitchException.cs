using System;

namespace NxB.BookingApi.Exceptions
{
    public class SwitchException : Exception
    {
        public SwitchException()
        {

        }

        public SwitchException(string message)
            : base(message)
        {
        }

        public SwitchException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}