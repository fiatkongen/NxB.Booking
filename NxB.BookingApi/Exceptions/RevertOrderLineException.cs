using System;
using System.Collections.Generic;
using System.Text;
using Munk.AspNetCore;
using Newtonsoft.Json;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Exceptions
{
    public class RevertOrderLineException : Exception
    {
        public RevertOrderLineException(OrderLine orderLine, string extraUserMessage = "")
            : base(
                $"Fejl ved tilbageførsel af linje {orderLine.Id} - {orderLine.Number} x {orderLine.Text} / stk {orderLine.PricePcs}. {extraUserMessage}")
        {
        }
    }
}
