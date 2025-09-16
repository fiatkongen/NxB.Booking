using System;
using System.Collections.Generic;
using System.Text;
using Munk.AspNetCore;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Exceptions
{
    public class MoveSubOrderException : Exception
    {
        public MoveSubOrderException(Guid subOrderId, Order order, Exception inner)
            : base(
                $"Fejl ved flytning af reservation med id {subOrderId} fra ordre {order.FriendlyId}", inner)
        {
        }
    }
}
