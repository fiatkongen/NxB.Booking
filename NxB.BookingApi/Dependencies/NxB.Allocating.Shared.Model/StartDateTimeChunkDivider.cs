using System;

namespace NxB.Allocating.Shared.Model
{
    public class StartDateTimeChunkDivider : IStartDateTimeChunkDivider
    {
        public virtual DateTime Today => DateTime.Today.ToEuTimeZone();
        public virtual DateTime MaxEndDate => Today.AddYears(6);
    }
}