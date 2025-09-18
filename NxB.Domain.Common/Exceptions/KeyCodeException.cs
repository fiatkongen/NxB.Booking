using System;

namespace NxB.Domain.Common.Exceptions
{
    public class KeyCodeException : Exception
    {
        public KeyCodeException()
        {
        }

        public KeyCodeException(string message)
            : base(message)
        {
        }

        public KeyCodeException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
