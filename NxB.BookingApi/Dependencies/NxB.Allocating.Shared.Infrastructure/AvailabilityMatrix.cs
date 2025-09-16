using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using Newtonsoft.Json;
using NxB.Allocating.Shared.Model;
using NxB.Allocating.Shared.Model.Exceptions;
using NxB.Domain.Common.Interfaces;

namespace NxB.Allocating.Shared.Infrastructure
{
    public class AvailabilityMatrix : ITenantEntity, IEntitySaved
    {
        private const int DICTIONARY_INITIAL_SIZE = 1000;

        public string Id { get; set; }
        public Guid TenantId { get; set; }
        public Dictionary<string, AvailablityArray> AvailabilityArrays { get; private set; } = new(DICTIONARY_INITIAL_SIZE);
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }
        public string AvailabilityArraysJson { get; set; }
        public bool IsSeeded { get; set; }

        [JsonConstructor]
        public AvailabilityMatrix(string id, Guid tenantId, DateTime start, DateTime end, string availabilityArraysJson = null)
        {
            Id = id;
            TenantId = tenantId;
            Start = start;
            End = end;

            if (availabilityArraysJson != null)
            {
                AvailabilityArraysJson = availabilityArraysJson;
                DeserializeFromJson();
            }
        }

        /* TO be used when it is possible for .net core to select EF constructor
        [JsonConstructor]
        public AvailabilityMatrix(string id, Guid tenantId, DateTime start, DateTime end, string availablityArraysJson) : this(id, tenantId, start, end)
        {
            AvailablityArraysJson = availablityArraysJson;
            DeserializeFromJson();
        }

        public AvailabilityMatrix(string id, Guid tenantId, DateTime start, DateTime end) 
        {
            Id = id;
            TenantId = tenantId;
            Start = start;
            End = end;
        }*/

        public void AddAllocations(CacheAllocation[] cacheAllocations)
        {
            var unitGrouped = cacheAllocations.GroupBy(x => x.ResourceId, x => x, (key, a) => new { unitId = key, cacheAllocations = a.ToList() });
            foreach (var unitGroup in unitGrouped)
            {
                var availablityArray = GetOrCreateAvailabilityArray(unitGroup.unitId);
                availablityArray.AddAllocations(unitGroup.cacheAllocations.ToArray());
            }
        }

        public decimal[] GetAvailabilityArray(string unitId, DateTime start, DateTime end)
        {
            GuardInterval(start, end);
            var availabilityArray = GetOrCreateAvailabilityArray(unitId);
            var array = availabilityArray.GetAvailabilityArray(start, end);
            return array;
        }

        public DateInterval[] GetPositiveDateIntervals(string unitId, DateTime start, DateTime end)
        {
            var availabilityArray = GetAvailabilityArray(unitId, start, end);
            var dateIntervals = new List<DateInterval>();

            var currentDate = start;

            DateInterval currentDateInterval = null;

            for (var i = 0; i < availabilityArray.Length; i++)
            {
                var isPositive = availabilityArray[i] > 0;
                if (isPositive)
                {
                    if (currentDateInterval == null)
                    {
                        currentDateInterval = new DateInterval(currentDate, currentDate.AddDays(1));
                    }
                    else
                    {
                        currentDateInterval = new DateInterval(currentDateInterval.Start, currentDate.AddDays(1));
                    }
                }
                else
                {
                    if (currentDateInterval != null)
                    {
                        dateIntervals.Add(currentDateInterval);
                        currentDateInterval = null;
                    }
                }
                currentDate = currentDate.AddDays(1);
            }

            if (currentDateInterval != null)
            {
                dateIntervals.Add(currentDateInterval);
            }


            return dateIntervals.ToArray();
        }

        public Dictionary<string, DateInterval[]> GetPositiveDateIntervalsForAllResources(DateTime start, DateTime end)
        {
            var dateIntervals = AvailabilityArrays.Select(x => new
            { key = x.Key, dateIntervals = GetPositiveDateIntervals(x.Key, start, end) }).ToDictionary(x => x.key, x => x.dateIntervals);
            return dateIntervals;
        }

        public DateInterval[] GetNegativeDateIntervals(string unitId, DateTime start, DateTime end)
        {
            var availabilityArray = GetAvailabilityArray(unitId, start, end);
            var dateIntervals = new List<DateInterval>();

            var currentDate = start;

            DateInterval currentDateInterval = null;

            for (var i = 0; i < availabilityArray.Length; i++)
            {
                var isNegative = availabilityArray[i] <= 0;
                if (isNegative)
                {
                    if (currentDateInterval == null)
                    {
                        currentDateInterval = new DateInterval(currentDate, currentDate.AddDays(1));
                    }
                    else
                    {
                        currentDateInterval = new DateInterval(currentDateInterval.Start, currentDate.AddDays(1));
                    }
                }
                else
                {
                    if (currentDateInterval != null)
                    {
                        dateIntervals.Add(currentDateInterval);
                        currentDateInterval = null;
                    }
                }
                currentDate = currentDate.AddDays(1);
            }

            if (currentDateInterval != null)
            {
                dateIntervals.Add(currentDateInterval);
            }


            return dateIntervals.ToArray();
        }

        public Dictionary<string, DateInterval[]> GetNegativeDateIntervalsForAllResources(DateTime start, DateTime end)
        {
            var dateIntervals = AvailabilityArrays.Select(x => new
                { key = x.Key, dateIntervals = GetNegativeDateIntervals(x.Key, start, end) }).ToDictionary(x => x.key, x => x.dateIntervals);
            return dateIntervals;
        }

        public bool CheckAvailability(string unitId, int minimumAvailability, DateTime start, DateTime end)
        {
            GuardInterval(start, end);
            var availabilityArray = GetOrCreateAvailabilityArray(unitId);
            var isAvailable = availabilityArray.GetAvailability(start, end) >= minimumAvailability;
            return isAvailable;
        }

        //public void ShrinkAvailabilityArrays(List<string> validResourceIds)
        //{
        //    foreach (var availabilityArray in this.AvailabilityArrays)
        //    {
        //        if (!validResourceIds.Contains(availabilityArray.Key))
        //        {
        //            AvailabilityArrays.Remove(availabilityArray.Key);
        //        }
        //    }

        //    SerializeToJson();
        //}

        private void GuardInterval(DateTime start, DateTime end)
        {
            if (start >= end) throw new AvailabilityException($@"Availablity request start={start.ToDanishDate()} must be greater than {end.ToDanishDate()}");
        }

        private AvailablityArray GetOrCreateAvailabilityArray(string unitId)
        {
            var exists = AvailabilityArrays.TryGetValue(unitId, out var availabilityArray);
            if (!exists)
            {
                availabilityArray = new AvailablityArray(Start, End);
                AvailabilityArrays.Add(unitId, availabilityArray);
            }
            return availabilityArray;
        }

        private void SerializeToJson()
        {
            this.AvailabilityArraysJson = JsonConvert.SerializeObject(this.AvailabilityArrays);
        }

        private void DeserializeFromJson()
        {
            this.AvailabilityArrays = JsonConvert.DeserializeObject<Dictionary<string, AvailablityArray>>(this.AvailabilityArraysJson);
        }

        public void OnEntitySaved(EntityState entityState)
        {
            SerializeToJson();
        }
    }
}
