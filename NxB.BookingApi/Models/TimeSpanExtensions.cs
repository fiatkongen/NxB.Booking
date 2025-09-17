using System;
using System.Collections.Generic;
using System.Linq;

namespace NxB.BookingApi.Models
{
    public static class TimeSpanExtensions
    {
        public static bool IsDayValid(this List<TimeSpanResult> timeSpanResults, DateTime day, string resourceId, Guid tenantId)
        {
            //timeSpanResults.Any()
            return false;
        }
    }
}
