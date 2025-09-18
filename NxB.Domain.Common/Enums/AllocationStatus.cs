namespace NxB.Domain.Common.Enums
{
    public enum AllocationStatus
    {
        None = 0,
        Cancelled = 1,
        NotArrived = 10,
        DelayedArrival = 11,
        Arrived = 12,
        NotDeparted = 20,
        DelayedDeparture = 21,
        Departed = 22,
    }

    public enum ArrivalStatus
    {
        None = 0,
        Cancelled = 1,
        NotArrived = 10,
        DelayedArrival = 11,
        Arrived = 12,
        NoShow = 13
    }

    public enum DepartureStatus
    {
        None = 0,
        Cancelled = 1,
        NotDeparted = 20,
        DelayedDeparture = 21,
        Departed = 22,
    }
}