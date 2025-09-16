using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Extensions
{
    public static class SubOrderExtensions
    {
        public static DateInterval DateInterval(this IEnumerable<SubOrder> subOrders)
        {
            var subOrdersList = subOrders.ToList();
            if (subOrdersList.Count == 0)
                return null;

            return new DateInterval(subOrdersList.Min(x => x.DateInterval.Start), subOrdersList.Max(x => x.DateInterval.End));
        }
    }
}
