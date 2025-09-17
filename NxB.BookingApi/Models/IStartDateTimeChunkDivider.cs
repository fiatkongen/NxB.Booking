using System;

namespace NxB.BookingApi.Models
{
    public interface IStartDateTimeChunkDivider
    {
        DateTime Today { get; }
        DateTime MaxEndDate { get; }
    }
}