using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using NxB.Allocating.Shared.Model;

namespace NxB.Allocating.Shared.Infrastructure
{
    public class TimeSpanService<T> : ITimeSpanService<T> where T : TimeSpanBase
    {
        private readonly ITimeSpanRepository _timeSpanRepository;
        private readonly TelemetryClient _telemetryClient;

        public TimeSpanService(ITimeSpanRepository timeSpanRepository, TelemetryClient telemetryClient)
        {
            _timeSpanRepository = timeSpanRepository;
            _telemetryClient = telemetryClient;
        }

        public async Task<List<TimeSpanResult>> CalculateOpenTimeSpans(DateInterval dateInterval, Guid tenantId)
        {
            var timeSpans = await _timeSpanRepository.FindAllWithinForTenant<T>(tenantId, dateInterval);
            var openClosedTimeSpanValidator = new OpenClosedTimeSpanValidator<T>(tenantId, timeSpans, null, null);
            var result = openClosedTimeSpanValidator.CalculateOpenTimeSpans(dateInterval);
            return result;
        }

        public async Task<List<TimeSpanResult>> CalculateClosedTimeSpans(DateInterval dateInterval, Guid tenantId, bool includeArrivalDepartureDays)
        {
            var timeSpans = await _timeSpanRepository.FindAllWithinForTenant<T>(tenantId, dateInterval);
            var tenantTimeSpans = timeSpans.OfType<TenantTimeSpan>().Cast<TimeSpanBase>().ToList();
            var groupedTimeSpans = timeSpans.GroupBy(x => x.ResourceId).ToList();

            List<TimeSpanResult> result = new();
            groupedTimeSpans.ForEach(gts =>
            {
                var timeSpansForResource = gts.ToList().Concat(tenantTimeSpans.Cast<T>()).ToList();
                var openClosedTimeSpanValidator =
                    new OpenClosedTimeSpanValidator<T>(tenantId, timeSpansForResource, null, null);
                result = result.Concat(openClosedTimeSpanValidator.CalculateClosedTimeSpans(dateInterval)).ToList();

                if (includeArrivalDepartureDays)
                {
                    var dayOpenClosedTimeSpanValidator = new ArrivalDepartureTimeSpanValidator<T>(tenantId, timeSpansForResource,
                        new List<TimeSpanSearchResult>());
                    result = result.Concat(dayOpenClosedTimeSpanValidator.BuildOpenTimeSpans(dateInterval)).ToList();
                }
            });

            return result;
        }

        public Task<List<T>> FindAllWithinForTenant(Guid tenantId, DateInterval dateInterval)
        {
            return _timeSpanRepository.FindAllWithinForTenant<T>(tenantId, dateInterval);
        }


        public async Task<List<TimeSpanSearchResult>> ValidateTimeSpans(DateInterval dateInterval, Guid tenantId, int monthsWidenCheck = 12)
        {
            var start = dateInterval.Start;
            var end = dateInterval.End;

            var startDate = start.AddMonths(0 - monthsWidenCheck).Highest(DateTime.Now.Date);
            if (startDate > dateInterval.End)   //added because of unit tests
            {
                startDate = dateInterval.Start.AddMonths(0 - monthsWidenCheck);
            }
            var wideDateInterval = new DateInterval(startDate, end.AddMonths(monthsWidenCheck));
            var allTimeSpans = await _timeSpanRepository.FindAllWithinForTenant<T>(tenantId, wideDateInterval);
            var timeSpanSearchResults = new List<TimeSpanSearchResult>();
            await RunTimeSpanValidators(dateInterval, tenantId, allTimeSpans, wideDateInterval, timeSpanSearchResults);
            return timeSpanSearchResults;
        }

        private async Task RunTimeSpanValidators(DateInterval dateInterval, Guid tenantId, List<T> allTimeSpans,
            DateInterval wideDateInterval, List<TimeSpanSearchResult> timeSpanSearchResults)
        {
            var timeSpanValidators = new List<TimeSpanValidator<T>>
            {
                new OpenClosedTimeSpanValidator<T>(tenantId, allTimeSpans, wideDateInterval, timeSpanSearchResults),
                new ArrivalDayTimeSpanValidator<T>(tenantId, allTimeSpans, timeSpanSearchResults),
                new ArrivalDepartureTimeSpanValidator<T>(tenantId, allTimeSpans, timeSpanSearchResults),
                new DepartureDayTimeSpanValidator<T>(tenantId, allTimeSpans, timeSpanSearchResults),
                new MinDaysTimeSpanValidator<T>(tenantId, allTimeSpans, timeSpanSearchResults),
                new MaxDaysTimeSpanValidator<T>(tenantId, allTimeSpans, timeSpanSearchResults)
            };

            foreach (var timeSpanValidator in timeSpanValidators)
            {
                try
                {
                    await timeSpanValidator.Validate(dateInterval);
                }
                catch (Exception exception)
                {
                    _telemetryClient.TrackException(exception);
                }
            }
        }

        private async Task<bool> RunAndStopTimeSpanValidators(DateInterval dateInterval, Guid tenantId, List<T> allTimeSpans)
        {
            var timeSpanSearchResults = new List<TimeSpanSearchResult>();
            TimeSpanValidator<T> timeSpanValidator = new OpenClosedTimeSpanValidator<T>(tenantId, allTimeSpans, dateInterval, timeSpanSearchResults);
            if (!await RunTimeSpanValidator(timeSpanValidator, dateInterval, timeSpanSearchResults)) 
                return false;

            timeSpanSearchResults = new();
            timeSpanValidator = new ArrivalDayTimeSpanValidator<T>(tenantId, allTimeSpans, timeSpanSearchResults);
            if (!await RunTimeSpanValidator(timeSpanValidator, dateInterval, timeSpanSearchResults)) return false;

            timeSpanSearchResults = new();
            timeSpanValidator = new ArrivalDepartureTimeSpanValidator<T>(tenantId, allTimeSpans, timeSpanSearchResults);
            if (!await RunTimeSpanValidator(timeSpanValidator, dateInterval, timeSpanSearchResults)) return false;

            timeSpanSearchResults = new();
            timeSpanValidator = new DepartureDayTimeSpanValidator<T>(tenantId, allTimeSpans, timeSpanSearchResults);
            if (!await RunTimeSpanValidator(timeSpanValidator, dateInterval, timeSpanSearchResults)) return false;

            timeSpanSearchResults = new();
            timeSpanValidator = new MinDaysTimeSpanValidator<T>(tenantId, allTimeSpans, timeSpanSearchResults);
            if (!await RunTimeSpanValidator(timeSpanValidator, dateInterval, timeSpanSearchResults)) return false;

            timeSpanSearchResults = new();
            timeSpanValidator = new MaxDaysTimeSpanValidator<T>(tenantId, allTimeSpans, timeSpanSearchResults);
            if (!await RunTimeSpanValidator(timeSpanValidator, dateInterval, timeSpanSearchResults)) return false;

            return true;
        }

        private async Task<bool> RunTimeSpanValidator(TimeSpanValidator<T> timeSpanValidator, DateInterval dateInterval, List<TimeSpanSearchResult> timeSpanSearchResults)
        {
            try
            {
                await timeSpanValidator.Validate(dateInterval);
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackException(exception);
                return true;
            }

            return (timeSpanSearchResults.All(x => x.IsAvailable));
        }

        public async Task<bool> ValidateTimeSpan(DateInterval dateInterval, List<T> allTimeSpans, Guid tenantId)
        {
            return await RunAndStopTimeSpanValidators(dateInterval, tenantId, allTimeSpans);
        }


        //combine with upper
        public async Task<List<TimeSpanSearchResult>> ValidateRulesTimeSpans(DateInterval dateInterval, Guid tenantId, int monthsWidenCheck = 12)
        {
            var start = dateInterval.Start;
            var end = dateInterval.End;

            var startDate = start.AddMonths(0 - monthsWidenCheck).Highest(DateTime.Now.Date);
            if (startDate > dateInterval.End)   //added because of unit tests
            {
                startDate = dateInterval.Start.AddMonths(0 - monthsWidenCheck);
            }
            var wideDateInterval = new DateInterval(startDate, end.AddMonths(monthsWidenCheck));
            var allTimeSpans = await _timeSpanRepository.FindAllWithinForTenant<T>(tenantId, wideDateInterval);
            var timeSpanSearchResults = new List<TimeSpanSearchResult>();
            var timeSpanValidators = new List<TimeSpanValidator<T>>
            {
                new ArrivalDayTimeSpanValidator<T>(tenantId, allTimeSpans, timeSpanSearchResults),
                new DepartureDayTimeSpanValidator<T>(tenantId, allTimeSpans, timeSpanSearchResults),
                new MinDaysTimeSpanValidator<T>(tenantId, allTimeSpans, timeSpanSearchResults),
                new MaxDaysTimeSpanValidator<T>(tenantId, allTimeSpans, timeSpanSearchResults)
            };

            foreach (var timeSpanValidator in timeSpanValidators)
            {
                try
                {
                    await timeSpanValidator.Validate(dateInterval);
                }
                catch (Exception exception)
                {
                    _telemetryClient.TrackException(exception);
                }

            }
            return timeSpanSearchResults;
        }
    }
}
