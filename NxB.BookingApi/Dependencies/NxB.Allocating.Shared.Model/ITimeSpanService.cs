using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NxB.Allocating.Shared.Model
{
    public interface ITimeSpanService<T> where T : TimeSpanBase
    {
        Task<List<TimeSpanSearchResult>> ValidateTimeSpans(DateInterval dateInterval, Guid tenantId, int monthsWidenCheck = 12);
        Task<bool> ValidateTimeSpan(DateInterval dateInterval, List<T> allTimeSpans, Guid tenantId);
        Task<List<TimeSpanSearchResult>> ValidateRulesTimeSpans(DateInterval dateInterval, Guid tenantId, int monthsWidenCheck = 12);
        Task<List<TimeSpanResult>> CalculateOpenTimeSpans(DateInterval dateInterval, Guid tenantId);
        Task<List<TimeSpanResult>> CalculateClosedTimeSpans(DateInterval dateInterval, Guid tenantId, bool includeArrivalDepartureDays);
        Task<List<T>> FindAllWithinForTenant(Guid tenantId, DateInterval dateInterval);
    }
}
