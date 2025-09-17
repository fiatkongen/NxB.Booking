using System;
using System.Collections.Generic;
using System.Linq;
using Itenso.TimePeriod;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    public abstract class TimeChunkDivider : ITimeChunkDivider
    {
        protected readonly IClaimsProvider ClaimsProvider;
        protected readonly IStartDateTimeChunkDivider _dateTimeChunkDivider = new StartDateTimeChunkDivider();
        protected readonly string _keySuffix;

        protected TimeChunkDivider(IClaimsProvider claimsProvider, IStartDateTimeChunkDivider dateTimeChunkDivider, string keySuffix)
        {
            ClaimsProvider = claimsProvider;
            _dateTimeChunkDivider = dateTimeChunkDivider;
            _keySuffix = keySuffix;
        }

        protected TimeChunkDivider(IClaimsProvider claimsProvider, string keySuffix)
        {
            ClaimsProvider = claimsProvider;
            _keySuffix = keySuffix;
        }

        protected TimeChunkDivider(IClaimsProvider claimsProvider) : this(claimsProvider, "")
        {

        }

        public abstract List<TimeChunk> BuildTimeChunks(DateTime start, DateTime end);
    }

    public class TimeChunkMonthDivider : TimeChunkDivider
    {
        private static int MaxMonths = 5 * 12;

        public TimeChunkMonthDivider(IClaimsProvider claimsProvider, string keySuffix) : base(claimsProvider, keySuffix)
        {
        }

        public TimeChunkMonthDivider(IClaimsProvider claimsProvider) : base(claimsProvider)
        {
        }

        public TimeChunkMonthDivider(IClaimsProvider claimsProvider, IStartDateTimeChunkDivider dateTimeChunkDivider, string keySuffix) : base(claimsProvider, dateTimeChunkDivider, keySuffix)
        {
        }

        public override List<TimeChunk> BuildTimeChunks(DateTime start, DateTime end)
        {
            var timeChunks = new List<TimeChunk>();
            var currentDate = start;
            var tenantId = ClaimsProvider.GetTenantId();

            while (currentDate < end)
            {
                string key = tenantId + "-" + currentDate.Month.ToString("00") + "_" + currentDate.Year + GetKeySuffix();
                var startDateChunk = new DateTime(currentDate.Year, currentDate.Month, 1);
                var endDateChunk = startDateChunk.AddMonths(1);
                timeChunks.Add(new TimeChunk(startDateChunk, endDateChunk, key));
                currentDate = endDateChunk;
            }
            var result = timeChunks.Where(x => x.IsTimeBlockWithin(start, end)).ToList();
            if (result.Count > MaxMonths)
            {
                throw new ArgumentOutOfRangeException("Max søgeinterval er dags dato + 5 år");
            }
            return result;
        }

        private string GetKeySuffix()
        {
            return String.IsNullOrEmpty(_keySuffix) ? "" : "_" + _keySuffix;
        }
    }

    public class TimeChunkYearDivider : TimeChunkDivider
    {
        public TimeChunkYearDivider(IClaimsProvider claimsProvider) : base(claimsProvider)
        {
        }

        public TimeChunkYearDivider(IClaimsProvider claimsProvider, string keySuffix) : base(claimsProvider, keySuffix)
        {
        }

        public TimeChunkYearDivider(IClaimsProvider claimsProvider, IStartDateTimeChunkDivider dateTimeChunkDivider, string keySuffix) : base(claimsProvider, dateTimeChunkDivider, keySuffix)
        {
        }

        public override List<TimeChunk> BuildTimeChunks(DateTime start, DateTime end)
        {
            var timeChunks = new List<TimeChunk>();
            var currentDate = start;
            var tenantId = ClaimsProvider.GetTenantId();

            while (currentDate < end)
            {
                string key = tenantId + "_" + currentDate.Year + GetKeySuffix();
                var startDateChunk = new DateTime(currentDate.Year, 1, 1);
                var endDateChunk = startDateChunk.AddYears(1);
                timeChunks.Add(new TimeChunk(startDateChunk, endDateChunk, key));
                currentDate = endDateChunk;
            }
            return timeChunks.Where(x => x.IsTimeBlockWithin(start, end)).ToList();
        }

        private string GetKeySuffix()
        {
            return String.IsNullOrEmpty(_keySuffix) ? "" : "_" + _keySuffix;
        }
    }

    public class TimeChunk
    {
        public readonly DateTime Start;
        public readonly DateTime End;
        public readonly string Key;

        public TimeChunk(DateTime start, DateTime end, string key)
        {
            Start = start;
            End = end;
            Key = key;
        }

        public bool IsTimeBlockWithin(DateTime start, DateTime end)
        {
            var timeChunkBlock = new TimeBlock(Start, End);
            var overlapsWith = timeChunkBlock.OverlapsWith(new TimeBlock(start, end));
            return overlapsWith;
        }
    }

}