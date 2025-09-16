using System;

namespace NxB.BookingApi.Exceptions
{
    public class CreateOnlineOrderException : Exception
    {
        public CreateOnlineOrderException()
        {
        }

        public CreateOnlineOrderException(string message)
            : base(message)
        {
        }

        public CreateOnlineOrderException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
