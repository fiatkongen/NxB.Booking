using System;

namespace NxB.Allocating.Shared.Model
{
    public interface IStartDateTimeChunkDivider
    {
        DateTime Today { get; }
        DateTime MaxEndDate { get; }
    }
}