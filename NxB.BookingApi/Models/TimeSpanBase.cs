using System;
using System.Collections.Generic;
using Itenso.TimePeriod;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public abstract class TimeSpanBase : ITenantEntity, IDateInterval
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int Sort { get; set; }
        public bool Disabled { get; set; }
        public TimeSpanType OpenClosed { get; set; } = TimeSpanType.Open;
        public bool IsOnlineOnly { get; set; }
        public string Type { get; set; }
        public string ParameterString { get; set; }
        public int? ParameterNumber { get; set; }
        public int GroupNo { get; set; }

        public abstract string ResourceId { get; }
        public ITimeBlock TimeBlock => new TimeBlock(Start, End);

        public abstract List<DateInterval> BuildDateIntervals();
    }

    [Serializable]
    public class RentalCategoryTimeSpan : TimeSpanBase
    {
        public Guid RentalCategoryId { get; set; }

        public override string ResourceId => this.RentalCategoryId.ToString();

        public RentalCategoryTimeSpan(Guid id, Guid tenantId, Guid rentalCategoryId)
        {
            Id = id;
            TenantId = tenantId;
            this.RentalCategoryId = rentalCategoryId;
        }

        public override List<DateInterval> BuildDateIntervals()
        {
            return new List<DateInterval> { new(Start, End) };
        }
    }

    [Serializable]
    public class TenantTimeSpan : TimeSpanBase
    {
        public override string ResourceId => this.TenantId.ToString();

        public TenantTimeSpan(Guid id, Guid tenantId)
        {
            Id = id;
            TenantId = tenantId;
        }

        public override List<DateInterval> BuildDateIntervals()
        {
            return new List<DateInterval> { new(Start, End) };
        }
    }

    //use same table? different types in same table, or maybe two different tables, sharing same codebase?
    //[Serializable]
    //public class PriceProfileTimeSpan : TimeSpanBase
    //{
    //    public Guid PriceProfileId { get; set; }
    //    // public PriceProfile
    //    public override string ResourceId => PriceProfileId.ToString();

    //    public PriceProfileTimeSpan(Guid id, Guid tenantId, Guid rentalCategoryId)
    //    {
    //        Id = id;
    //        TenantId = tenantId;
    //        // this.RentalCategoryId = rentalCategoryId;
    //    }
    //}
}
