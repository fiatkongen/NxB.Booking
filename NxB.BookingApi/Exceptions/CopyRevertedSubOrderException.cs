using System;
using System.Collections.Generic;
using System.Text;
using Munk.AspNetCore;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Exceptions
{
    public class CopyRevertedSubOrderException: Exception
    {
        public CopyRevertedSubOrderException(SubOrder subOrder, string extraUserMessage = "")
            : base(
                $"Fejl ved kopiering af ordrelinjer for del-ordre {subOrder.Id}.{extraUserMessage}")
        {
        }
    }
}
