using System;

namespace NxB.BookingApi.Models.Exceptions
{
    public class AvailabilityException : Exception
    {
        public AvailabilityException()
        {
        }

        public AvailabilityException(string message)
            : base(message)
        {
        }

        public AvailabilityException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class AvailabilityOverReleasedException : AvailabilityException
    {
        public Guid RentalUnitId { get; }

        public AvailabilityOverReleasedException(Guid rentalUnitId, string rentalUnitName) : base($"For meget frigivet for enhed {rentalUnitName}")
        {
            RentalUnitId = rentalUnitId;
        }
    }

    public class AvailabilityCacheNotInitialized : AvailabilityException
    {
        public DateTime Start { get; }
        public DateTime End { get; }

        public AvailabilityCacheNotInitialized(DateTime start, DateTime end) : base($"Cache for {start.ToDanishDate()} - {end.ToDanishDate()} is not initialized")
        {
            Start = start;
            End = end;
        }
    }

    public class AvailabilityCacheUnseeded : AvailabilityException
    {
        public DateTime Start { get; }
        public DateTime End { get; }

        public AvailabilityCacheUnseeded(DateTime start, DateTime end) : base($"Cache for  {start.ToDanishDate()} - {end.ToDanishDate()} is unseeded")
        {
            Start = start;
            End = end;
        }
    }
}
