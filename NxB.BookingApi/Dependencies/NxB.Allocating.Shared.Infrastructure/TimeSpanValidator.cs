using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Itenso.TimePeriod;
using Munk.AspNetCore;
using NxB.Allocating.Shared.Model;
using NxB.Domain.Common.Enums;
using NxB.Dto.AllocationApi;

namespace NxB.Allocating.Shared.Infrastructure
{
    public abstract class TimeSpanValidator<T> where T : TimeSpanBase
    {
        protected static string MATRIX_ID = "matrixId";

        protected readonly Guid _tenantId;
        protected readonly List<T> _allTimeSpans;
        private readonly List<TimeSpanSearchResult> _currentResults;

        protected TimeSpanValidator(Guid tenantId, List<T> allTimeSpans, List<TimeSpanSearchResult> currentResults)
        {
            this._tenantId = tenantId;
            _allTimeSpans = allTimeSpans;
            _currentResults = currentResults;
        }

        public abstract Task Validate(DateInterval dateInterval);
        public abstract List<TimeSpanResult> BuildOpenTimeSpans(DateInterval dateInterval);

        public virtual List<T> FilterRelevantTimeSpans(DateInterval dateInterval)
        {
            return _allTimeSpans.Where(x => x.TimeBlock.OverlapsWith(new TimeBlock(dateInterval.Start, dateInterval.End))).ToList();
        }

        public TimeSpanSearchResult SetResultForResource(string resourceId, bool isAvailable, DateInterval dateInterval)
        {
            var result = _currentResults.SingleOrDefault(x => x.ResourceId == resourceId);

            if (result != null && result.IsAvailable)
            {
                _currentResults.Remove(result);
                result = null;
            }

            if (result == null)
            {
                result = new TimeSpanSearchResult(resourceId, isAvailable, dateInterval.Start, dateInterval.End);
                _currentResults.Add(result);
            }

            return result;
        }
    }

    public class OpenClosedTimeSpanValidator<T> : TimeSpanValidator<T> where T : TimeSpanBase
    {
        private readonly DateInterval _wideDateInterval;

        public OpenClosedTimeSpanValidator(Guid tenantId, List<T> allTimeSpans, DateInterval wideDateInterval, List<TimeSpanSearchResult> currentResults) : base(tenantId, allTimeSpans, currentResults)
        {
            _wideDateInterval = wideDateInterval;
        }

        public override List<T> FilterRelevantTimeSpans(DateInterval dateInterval)
        {
            return base.FilterRelevantTimeSpans(dateInterval).Where(x => (x.OpenClosed == TimeSpanType.Closed || x.OpenClosed == TimeSpanType.Open)).ToList();
        }

        public override async Task Validate(DateInterval dateInterval)
        {
            var specificTimeSpans = FilterRelevantTimeSpans(dateInterval);
            ValidateTimeSpans(specificTimeSpans, dateInterval);
        }

        public override List<TimeSpanResult> BuildOpenTimeSpans(DateInterval dateInterval)
        {
            throw new NotImplementedException();
            //return this.CalculateClosedTimeSpans(dateInterval);
        }

        public List<TimeSpanResult> CalculateOpenTimeSpans(DateInterval dateInterval)
        {
            return CalculateTimeSpans(FilterRelevantTimeSpans(dateInterval), dateInterval, (availabilityMatrix) => availabilityMatrix.GetPositiveDateIntervalsForAllResources(dateInterval.Start, dateInterval.End), TimeSpanType.Open);
        }

        private List<TimeSpanResult> CalculateOpenTimeSpans(List<T> timeSpans, DateInterval dateInterval)
        {
            return CalculateTimeSpans(timeSpans, dateInterval, (availabilityMatrix) => availabilityMatrix.GetPositiveDateIntervalsForAllResources(dateInterval.Start, dateInterval.End), TimeSpanType.Open);
        }

        public List<TimeSpanResult> CalculateClosedTimeSpans(DateInterval dateInterval)
        {
            return CalculateTimeSpans(FilterRelevantTimeSpans(dateInterval), dateInterval, (availabilityMatrix) => availabilityMatrix.GetNegativeDateIntervalsForAllResources(dateInterval.Start, dateInterval.End), TimeSpanType.Closed);
        }

        private List<TimeSpanResult> CalculateClosedTimeSpans(List<T> timeSpans, DateInterval dateInterval)
        {
            return CalculateTimeSpans(timeSpans, dateInterval, (availabilityMatrix) => availabilityMatrix.GetNegativeDateIntervalsForAllResources(dateInterval.Start, dateInterval.End), TimeSpanType.Closed);
        }

        private void ValidateTimeSpans(List<T> dateIntervalTimeSpans, DateInterval dateInterval)
        {
            var groupedTimeSpans = dateIntervalTimeSpans.GroupBy(x => x.ResourceId);

            var allOpenTimeSpansForDateInterval = CalculateOpenTimeSpans(dateIntervalTimeSpans, dateInterval);

            foreach (var groupedTimeSpan in groupedTimeSpans)
            {
                var openTimeSpansForResource = allOpenTimeSpansForDateInterval.Where(x => x.ResourceId == groupedTimeSpan.Key).ToList();
                var available = (openTimeSpansForResource.Count == 1 && openTimeSpansForResource[0].GetTimeBlock().Equals(dateInterval.TimeBlock));

                var timeSpanSearchResult = SetResultForResource(groupedTimeSpan.Key, available, dateInterval);

                if (openTimeSpansForResource.Count != 1 ||
                    dateInterval.Start < openTimeSpansForResource.First().Start || //Closed on start date
                    dateInterval.End > openTimeSpansForResource.Last().End) //Closed on end date
                {
                    var wideInterval = new DateInterval(_wideDateInterval.Start, _wideDateInterval.End);
                    var allTimeSpansForResource = FilterRelevantTimeSpans(wideInterval).Where(x => x.ResourceId == groupedTimeSpan.Key).ToList();
                    var closedTimeSpans = CalculateClosedTimeSpans(allTimeSpansForResource, wideInterval);
                    closedTimeSpans.ForEach(x =>
                    {
                        x.UnknownStart = (x.Start == _wideDateInterval.Start);
                        x.UnknownEnd = (x.End == _wideDateInterval.End);
                    });
                    timeSpanSearchResult.InformationTimeSpans.AddRange(closedTimeSpans);
                }
            }
        }

        private List<TimeSpanResult> CalculateTimeSpans(List<T> timeSpans, DateInterval dateInterval, Func<AvailabilityMatrix, Dictionary<string, DateInterval[]>> getIntervals, TimeSpanType timeSpanType)
        {
            if (timeSpans.Count == 0)
                return new List<TimeSpanResult>();

            var availabilityMatrix = new AvailabilityMatrix(MATRIX_ID, Guid.Empty, dateInterval.Start, dateInterval.End);

            var seedAllocations = timeSpans.GroupBy(x => x.ResourceId).Select(x => new CacheAllocation(x.Key, new DateTime(2000, 1, 1), new DateTime(2050, 1, 1), 1)).ToArray();
            availabilityMatrix.AddAllocations(seedAllocations);

            //når open "invertes", skal der tages hensyn til at open fra og med bliver til closed IKKE fra og med,
            //if (timeSpanType == TimeSpanType.Closed)
            //{
            //    timeSpans.Where(x => x.OpenClosed == TimeSpanType.Open && x.Start > new DateTime(2000, 1, 1)).ToList().ForEach(x =>
            //    {
            //        x.End = x.End.AddDays(1);
            //    });
            //}

            var cacheAllocations = timeSpans.Select(x => new CacheAllocation(x.ResourceId, x.Start, x.End, x.OpenClosed == TimeSpanType.Closed ? -1 : 1)).ToArray();
            availabilityMatrix.AddAllocations(cacheAllocations);

            var dateIntervalGroups = getIntervals(availabilityMatrix);
            var result = dateIntervalGroups.SelectMany(x =>
                x.Value.Select(di => new TimeSpanResult(x.Key, di.Start, di.End, timeSpanType))).ToList();

            return result;
        }
    }

    public class DayTimeSpanValidator<T> : TimeSpanValidator<T> where T : TimeSpanBase
    {
        public DayTimeSpanValidator(Guid tenantId, List<T> allTimeSpans, List<TimeSpanSearchResult> currentResults) : base(tenantId, allTimeSpans, currentResults)
        {
        }

        public override List<T> FilterRelevantTimeSpans(DateInterval dateInterval)
        {
            var timeSpans = base.FilterRelevantTimeSpans(dateInterval).Where(x => (x.OpenClosed == TimeSpanType.ArrivalDay || x.OpenClosed == TimeSpanType.DepartureDay || x.OpenClosed == TimeSpanType.ArrivalAndDepartureDay)).ToList();

            return timeSpans; ;
        }

        public override Task Validate(DateInterval dateInterval)
        {
            throw new NotImplementedException();
        }

        public virtual List<DayOfWeek> GetDaysOfWeek(T timeSpan)
        {
            if (timeSpan.OpenClosed == TimeSpanType.ArrivalDay || timeSpan.OpenClosed == TimeSpanType.DepartureDay)
            {
                return new List<DayOfWeek> { this.GetDayOfWeek(timeSpan) };
            }
            else
            {
                var arrDays = timeSpan.ParameterString.Split(',');
                return new List<DayOfWeek> { ((DayOfWeek)int.Parse(arrDays[0])), ((DayOfWeek)int.Parse(arrDays[1])) };
            }
        }

        public virtual DayOfWeek GetDayOfWeek(T timeSpan)
        {
            return  (DayOfWeek)timeSpan.ParameterNumber.Value;
        }

        public override List<TimeSpanResult> BuildOpenTimeSpans(DateInterval dateInterval)
        {
            var timeSpans = FilterRelevantTimeSpans(dateInterval);
            if (timeSpans.Count == 0)
                return new List<TimeSpanResult>();

            var availabilityMatrix = new AvailabilityMatrix(MATRIX_ID, Guid.Empty, dateInterval.Start, dateInterval.End);
            var groupedTimeSpans = timeSpans.GroupBy(x => x.ResourceId).ToList();

            List<TimeSpanResult> result = new();
            groupedTimeSpans.ForEach(gts =>
            {
                var currentDate = dateInterval.Start;
                while (currentDate < dateInterval.End)
                {
                    var currentDateTimeSpanBlock = new TimeBlock(currentDate, currentDate.AddDays(1));
                    if (gts.None(x => x.TimeBlock.OverlapsWith(currentDateTimeSpanBlock)) || gts.Where(x => x.TimeBlock.OverlapsWith(currentDateTimeSpanBlock)).Any(x => GetDaysOfWeek(x).Any(day => day == currentDate.DayOfWeek)))
                    {
                        availabilityMatrix.AddAllocations(new[] { new CacheAllocation(gts.Key, currentDateTimeSpanBlock.Start, currentDateTimeSpanBlock.End, 1) });
                    }
                    currentDate = currentDate.AddDays(1);
                }

                var dateIntervalGroups = availabilityMatrix.GetNegativeDateIntervalsForAllResources(dateInterval.Start, dateInterval.End);
                var tmpResult = dateIntervalGroups.SelectMany(x =>
                    x.Value.Select(di => new TimeSpanResult(x.Key, di.Start, di.End, TimeSpanType.Closed))).ToList();
                result = result.Concat(tmpResult).ToList();
            });
            return result;
        }

    }

    public class ArrivalDayTimeSpanValidator<T> : DayTimeSpanValidator<T> where T : TimeSpanBase
    {
        public ArrivalDayTimeSpanValidator(Guid tenantId, List<T> allTimeSpans, List<TimeSpanSearchResult> currentResults) : base(tenantId, allTimeSpans, currentResults)
        {
        }

        public override async Task Validate(DateInterval dateInterval)
        {
            var timeSpans = FilterRelevantTimeSpans(dateInterval);
            var groupedTimeSpans = timeSpans.GroupBy(x => x.ResourceId).ToList();

            groupedTimeSpans.ForEach(gts =>
            {
                {
                    if (gts.Any(x => GetDayOfWeek(x) == dateInterval.Start.DayOfWeek))
                    {
                        SetResultForResource(gts.Key, true, dateInterval);
                        return;
                    }

                    foreach (var timeSpan in gts)
                    {
                        var dayOfWeek = GetDayOfWeek(timeSpan);
                        if (dayOfWeek != dateInterval.Start.DayOfWeek)
                        {
                            var result = SetResultForResource(gts.Key, false, dateInterval);
                            var timeSpanResult = new TimeSpanResult(gts.Key, dateInterval.Start, dateInterval.Start.AddDays(1), TimeSpanType.ArrivalDay);
                            timeSpanResult.ParameterNumber = (int)dayOfWeek;
                            result.InformationTimeSpans.Add(timeSpanResult);
                        }
                    }
                }
            });
        }

        public override DayOfWeek GetDayOfWeek(T timeSpan)
        {
            return (DayOfWeek)timeSpan.ParameterNumber.Value;
        }

        public override List<T> FilterRelevantTimeSpans(DateInterval dateInterval)
        {
            return base.FilterRelevantTimeSpans(dateInterval).Where(x => x.OpenClosed == TimeSpanType.ArrivalDay).ToList();
        }
    }

    public class DepartureDayTimeSpanValidator<T> : DayTimeSpanValidator<T> where T : TimeSpanBase
    {
        public DepartureDayTimeSpanValidator(Guid tenantId, List<T> allTimeSpans, List<TimeSpanSearchResult> currentResults) : base(tenantId, allTimeSpans, currentResults)
        {
        }

        public override async Task Validate(DateInterval dateInterval)
        {
            var timeSpans = FilterRelevantTimeSpans(dateInterval);
            var groupedTimeSpans = timeSpans.GroupBy(x => x.ResourceId).ToList();

            groupedTimeSpans.ForEach(gts =>
            {
                {
                    if (gts.Any(x => GetDayOfWeek(x) == dateInterval.End.DayOfWeek))
                    {
                        SetResultForResource(gts.Key, true, dateInterval);
                        return;
                    }

                    foreach (var timeSpan in gts)
                    {
                        var dayOfWeek = GetDayOfWeek(timeSpan);
                        if (dayOfWeek != dateInterval.End.DayOfWeek)
                        {
                            var result = SetResultForResource(gts.Key, false, dateInterval);
                            var timeSpanResult = new TimeSpanResult(gts.Key, dateInterval.Start, dateInterval.Start.AddDays(1), TimeSpanType.DepartureDay);
                            timeSpanResult.ParameterNumber = (int)dayOfWeek;
                            result.InformationTimeSpans.Add(timeSpanResult);
                        }
                    }
                }
            });
        }

        public override DayOfWeek GetDayOfWeek(T timeSpan)
        {
            return (DayOfWeek)timeSpan.ParameterNumber.Value;
        }

        public override List<T> FilterRelevantTimeSpans(DateInterval dateInterval)
        {
            return base.FilterRelevantTimeSpans(dateInterval).Where(x => x.OpenClosed == TimeSpanType.DepartureDay).ToList();
        }
    }

    public class ArrivalDepartureTimeSpanValidator<T> : DayTimeSpanValidator<T> where T : TimeSpanBase
    {
        public ArrivalDepartureTimeSpanValidator(Guid tenantId, List<T> allTimeSpans, List<TimeSpanSearchResult> currentResults) : base(tenantId, allTimeSpans, currentResults)
        {
        }

        public override async Task Validate(DateInterval dateInterval)
        {
            var timeSpans = FilterRelevantTimeSpans(dateInterval);
            var groupedTimeSpans = timeSpans.GroupBy(x => x.ResourceId).ToList();

            groupedTimeSpans.ForEach(gts =>
            {
                {
                    if (gts.Any(x => GetArrivalDayOfWeek(x) == dateInterval.Start.DayOfWeek && GetDepartureDayOfWeek(x) == dateInterval.End.DayOfWeek))
                    {
                        SetResultForResource(gts.Key, true, dateInterval);
                        return;
                    }

                    foreach (var timeSpan in gts)
                    {
                        var arrivalDayOfWeek = GetArrivalDayOfWeek(timeSpan);
                        var departureDayOfWeek = GetDepartureDayOfWeek(timeSpan);
                        if (arrivalDayOfWeek != dateInterval.Start.DayOfWeek || departureDayOfWeek != dateInterval.End.DayOfWeek)
                        {
                            var result = SetResultForResource(gts.Key, false, dateInterval);
                            var timeSpanResult = new TimeSpanResult(gts.Key, dateInterval.Start, dateInterval.Start.AddDays(1), timeSpan.OpenClosed);
                            timeSpanResult.ParameterString = timeSpan.ParameterString;
                            result.InformationTimeSpans.Add(timeSpanResult);
                        }
                    }
                }
            });
        }

        public DayOfWeek GetArrivalDayOfWeek(T timeSpan)
        {
            return (DayOfWeek)int.Parse(timeSpan.ParameterString.Split(',')[0]);
        }

        public DayOfWeek GetDepartureDayOfWeek(T timeSpan)
        {
            return (DayOfWeek)int.Parse(timeSpan.ParameterString.Split(',')[1]);
        }

        public override DayOfWeek GetDayOfWeek(T timeSpan)
        {
            throw new NotImplementedException();
        }

        public override List<T> FilterRelevantTimeSpans(DateInterval dateInterval)
        {
            return base.FilterRelevantTimeSpans(dateInterval).Where(x => x.OpenClosed == TimeSpanType.ArrivalAndDepartureDay).ToList();
        }
    }

    public class MinDaysTimeSpanValidator<T> : TimeSpanValidator<T> where T : TimeSpanBase
    {
        public MinDaysTimeSpanValidator(Guid tenantId, List<T> allTimeSpans, List<TimeSpanSearchResult> currentResults) : base(tenantId, allTimeSpans, currentResults)
        {
        }

        public override async Task Validate(DateInterval dateInterval)
        {
            var timeSpans = FilterRelevantTimeSpans(dateInterval);

            foreach (var timeSpan in timeSpans)
            {
                var minDays = timeSpan.ParameterNumber.Value;
                if (dateInterval.TimeBlock.Duration.Days < minDays)
                {
                    var result = SetResultForResource(timeSpan.ResourceId, false, dateInterval);
                    var timeSpanResult = new TimeSpanResult(timeSpan.ResourceId, timeSpan.End, timeSpan.End.AddDays(minDays), TimeSpanType.MinDays);
                    timeSpanResult.ParameterNumber = minDays;
                    result.InformationTimeSpans.Add(timeSpanResult);
                }
            }
        }

        public override List<TimeSpanResult> BuildOpenTimeSpans(DateInterval dateInterval)
        {
            throw new NotImplementedException();
        }

        public override List<T> FilterRelevantTimeSpans(DateInterval dateInterval)
        {
            return base.FilterRelevantTimeSpans(dateInterval).Where(x => x.OpenClosed == TimeSpanType.MinDays).ToList();
        }
    }

    public class MaxDaysTimeSpanValidator<T> : TimeSpanValidator<T> where T : TimeSpanBase
    {
        public MaxDaysTimeSpanValidator(Guid tenantId, List<T> allTimeSpans, List<TimeSpanSearchResult> currentResults) : base(tenantId, allTimeSpans, currentResults)
        {
        }

        public override async Task Validate(DateInterval dateInterval)
        {
            var timeSpans = FilterRelevantTimeSpans(dateInterval);

            foreach (var timeSpan in timeSpans)
            {
                var maxDays = timeSpan.ParameterNumber.Value;
                if (dateInterval.TimeBlock.Duration.Days > maxDays)
                {
                    var result = SetResultForResource(timeSpan.ResourceId, false, dateInterval);
                    var timeSpanResult = new TimeSpanResult(timeSpan.ResourceId, timeSpan.End, timeSpan.End.AddDays(maxDays), TimeSpanType.MaxDays);
                    timeSpanResult.ParameterNumber = maxDays;
                    result.InformationTimeSpans.Add(timeSpanResult);
                }
            }
        }

        public override List<TimeSpanResult> BuildOpenTimeSpans(DateInterval dateInterval)
        {
            throw new NotImplementedException();
        }

        public override List<T> FilterRelevantTimeSpans(DateInterval dateInterval)
        {
            return base.FilterRelevantTimeSpans(dateInterval).Where(x => x.OpenClosed == TimeSpanType.MaxDays).ToList();
        }
    }
}
