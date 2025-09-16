using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using NxB.Domain.Common.Interfaces;
using NxB.BookingApi.Infrastructure;

namespace NxB.BookingApi.Models
{
    public interface IFriendlyOrderIdProvider
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        long GenerateNextFriendlyOrderId();
    }
}
