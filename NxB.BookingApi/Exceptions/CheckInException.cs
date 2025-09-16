using System;

namespace NxB.BookingApi.Exceptions
{
    public class CheckInException : Exception
    {
        public CheckInException()
        {
        }

        public CheckInException(string message)
            : base(message)
        {
        }

        public CheckInException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
