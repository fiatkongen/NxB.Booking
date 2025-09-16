using System;
using System.Collections.Generic;
using Itenso.TimePeriod;
using NxB.Domain.Common.Enums;

namespace NxB.Allocating.Shared.Model
{
    public class TimeSpanResult
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpanType OpenClosed { get; set; }
        public int? ParameterNumber { get; set; }
        public string ParameterString { get; set; }
        public string ResourceId { get; set; }
        public bool? UnknownStart { get; set; }
        public bool? UnknownEnd { get; set; }

        public TimeSpanResult(string resourceId, DateTime start, DateTime end, TimeSpanType openClosed)
        {
            Start = start;
            End = end;
            OpenClosed = openClosed;
            ResourceId = resourceId;
        }
    }

    public static class TimeSpanResultExtensions
    {
        public static ITimeBlock GetTimeBlock(this TimeSpanResult timeSpan)
        {
            return new TimeBlock(timeSpan.Start, timeSpan.End);
        }
    }

    public class TimeSpanSearchResult
    {
        public List<TimeSpanResult> InformationTimeSpans { get; set; } = new();
        public string ResourceId { get; set; }
        public bool IsAvailable { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public TimeSpanSearchResult(string resourceId, bool isAvailable, DateTime start, DateTime end)
        {
            ResourceId = resourceId;
            IsAvailable = isAvailable;
            Start = start;
            End = end;
        }
    }
}