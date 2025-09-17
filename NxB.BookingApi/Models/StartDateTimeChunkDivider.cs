using System;

namespace NxB.BookingApi.Models
{
    public class StartDateTimeChunkDivider : IStartDateTimeChunkDivider
    {
        public virtual DateTime Today => DateTime.Today.ToEuTimeZone();
        public virtual DateTime MaxEndDate => Today.AddYears(6);
    }
}