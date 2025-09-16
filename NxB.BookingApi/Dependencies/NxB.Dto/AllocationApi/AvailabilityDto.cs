using System;
using System.Collections.Generic;
using System.Text;
using Itenso.TimePeriod;

namespace NxB.Dto.AllocationApi
{
    public class AvailabilityDto
    {
        public Guid ResourceId { get; set; }
        public decimal Number { get; set; }
    }

    
    public abstract class AvailabilityDaysBase
    {
        public Guid ResourceId { get; set; }
        // public long? LegacyId { get; set; }
    }

    public class AvailabilityDaysDto : AvailabilityDaysBase
    {
        public int Days { get; set; }
    }

    public class AvailabilityDaysForDateIntervalDto
    {
        public List<AvailabilityDaysDto> BeforeStartDate { get; set; } = new();
        public List<AvailabilityDaysDto> AfterEndDate { get; set; } = new();
    }
}
