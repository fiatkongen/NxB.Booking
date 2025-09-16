using System;
using System.Collections.Generic;
using System.Linq;
using Itenso.TimePeriod;
using Newtonsoft.Json;
using NxB.Allocating.Shared.Model;
using NxB.Allocating.Shared.Model.Exceptions;

namespace NxB.Allocating.Shared.Infrastructure
{
    public class AvailablityArray
    {
        public List<decimal> Store { get; private set; }
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }
        public TimeSpan Duration => new TimeInterval(Start, End).Duration;

        [JsonConstructor]
        private AvailablityArray(DateTime start, DateTime end, List<decimal> store)
        {
            Start = start;
            End = end;
            Store = store;
        }

        public AvailablityArray(DateTime start, DateTime end) : this(start, end, new decimal[end.Date.Subtract(start.Date).Days].ToList())
        { }

        public void AddAllocations(IEnumerable<CacheAllocation> cacheAllocations)
        {
            var allocations = cacheAllocations.OrderBy(x => x.Start).ToArray();
            foreach (var allocation in allocations)
            {
                if (allocation.Start >= End || allocation.End <= Start)
                {
                    continue;
                };

                int startIndex = this.GetIndex(allocation.Start);
                int i = startIndex > 0 ? startIndex : 0;
                int duration = GetIndex(allocation.End);
                bool added = false;

                while (i < duration)
                {
                    Store[i++] += allocation.Number;
                    added = true;
                }

                if (!added)
                    throw new AvailabilityException(
                        $@"Allocation start={allocation.Start.ToDanishDate()}, end={allocation.End.ToDanishDate()} does not fall within range of AllocationArray start={Start.ToDanishDate()}, end={End.ToDanishDate()}");
            }
        }

        private int GetIndex(DateTime date)
        {
            int index = date.Date.Subtract(Start).Days;
            var durationDays = this.Duration.Days;

            if (index < 0)
            {
                index = 0;
            }
            else if (index > durationDays)
            {
                index = durationDays;
            }
            return index;
        }

        private DateTime GetDate(int index)
        {
            return Start.AddDays(index);
        }

        public decimal[] GetAvailabilityArray(DateTime start, DateTime end)
        {
            var startIndex = GetIndex(start);
            var endIndex = GetIndex(end).Lowest(GetIndex(End));
            var array = Store.Skip(startIndex).Take(endIndex - startIndex).ToArray();
            return array;
        }

        public List<CacheAllocation> GenerateAvailabilityCacheAllocations(string resourceId, DateTime start, DateTime end)
        {
            var availabilityCacheAllocations = new List<CacheAllocation>();
            if (start == end) return availabilityCacheAllocations;

            var availabilityArray = GetAvailabilityArray(start, end);

            int index = 0;
            int offsetIndex = GetIndex(start) - GetIndex(this.Start);
            decimal currentOccCount = availabilityArray[index];
            var currentDate = start;
            var nextDate = GetDate(index + offsetIndex + 1);

            while (nextDate < end)
            {
                var nextOccCount = availabilityArray[index + 1];
                if (nextOccCount != currentOccCount)
                {
                    if (nextOccCount == 0 && currentOccCount != 0)
                    {
                        availabilityCacheAllocations.Add(new CacheAllocation(resourceId, currentDate, nextDate, currentOccCount));
                        
                    }
                    else if (currentOccCount == 0)
                    {
                        currentDate = nextDate;
                    }
                    else
                    {
                        availabilityCacheAllocations.Add(new CacheAllocation(resourceId, currentDate, nextDate, currentOccCount));
                        currentDate = nextDate;
                    }
                }

                index++;
                currentOccCount = availabilityArray[index];
                nextDate = GetDate(index + offsetIndex + 1);
            }

            if (currentOccCount != 0)
                availabilityCacheAllocations.Add(new CacheAllocation(resourceId, currentDate, end, currentOccCount));

            return availabilityCacheAllocations;
        }

        public decimal GetAvailability(DateTime start, DateTime end)
        {
            var minNumber = GetAvailabilityArray(start, end).Min(x => x);
            return minNumber;
        }
    }
}