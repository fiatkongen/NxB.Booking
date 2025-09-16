using System;
using Itenso.TimePeriod;

namespace NxB.Allocating.Shared.Model
{
    public class CacheAllocation
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public decimal Number { get; set; }
        public string ResourceId { get; set; }
        public TimeSpan Duration => new TimeInterval(Start, End).Duration;

        public CacheAllocation(string resourceId, DateTime start, DateTime end, decimal number)
        {
            ResourceId = resourceId;
            Start = start;
            End = end;
            Number = number;
        }

        public CacheAllocation(Allocation allocation)
        {
            Start = allocation.Start;
            End = allocation.End;
            Number = allocation.Number;
            ResourceId = allocation.RentalUnitId.ToString();
        }
    }
}