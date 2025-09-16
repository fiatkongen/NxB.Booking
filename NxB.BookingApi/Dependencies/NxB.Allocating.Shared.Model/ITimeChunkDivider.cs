using System;
using System.Collections.Generic;

namespace NxB.Allocating.Shared.Model
{
    public interface ITimeChunkDivider
    {
        List<TimeChunk> BuildTimeChunks(DateTime start, DateTime end);
    }
}