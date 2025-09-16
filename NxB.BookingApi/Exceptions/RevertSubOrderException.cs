using System;
using System.Collections.Generic;
using System.Text;
using Munk.AspNetCore;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Exceptions
{
    public class RevertSubOrderException : Exception
    {
        public RevertSubOrderException(SubOrder subOrder, string extraUserMessage = "")
            : base(
                $"Fejl ved tilbageførsel af ordrelinjer for del-ordre {subOrder.Id}.{extraUserMessage}")
        {
        }
    }
}
