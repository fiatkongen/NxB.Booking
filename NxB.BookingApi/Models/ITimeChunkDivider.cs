using System;
using System.Collections.Generic;

namespace NxB.BookingApi.Models
{
    public interface ITimeChunkDivider
    {
        List<TimeChunk> BuildTimeChunks(DateTime start, DateTime end);
    }
}