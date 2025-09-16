using System;

namespace NxB.BookingApi.Exceptions
{
    public class VoucherException : Exception
    {
        public VoucherException()
        {
        }

        public VoucherException(string message)
            : base(message)
        {
        }

        public VoucherException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class VoucherAmountException : VoucherException
    {
        public VoucherAmountException()
        {
        }

        public VoucherAmountException(string message)
            : base(message)
        {
        }

        public VoucherAmountException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
