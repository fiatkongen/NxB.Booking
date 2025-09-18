namespace NxB.Domain.Common.Enums
{
    public enum JobTaskStatus
    {
        None = 0,
        Queued = 1,
        Running = 2,
        Stopped = 3,
        Completed = 4,
        Error = 5,
        Cancelled = 6
    }
}