using System;
using System.ComponentModel.DataAnnotations.Schema;
using Itenso.TimePeriod;
using Munk.Utils.Object;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class Allocation : ITenantEntity
    {
        public static DateInterval BaseAllocationDateInterval = new DateInterval(new DateTime(2018, 1, 1), new DateTime(2050, 1, 1));

        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }
        public decimal Number { get; set; }
        public TimeSpan Duration => new TimeInterval(Start, End).Duration;
        public Guid RentalUnitId { get; set; }
        public string RentalUnitName { get; set; }

        public DateInterval DateInterval
        {
            get => new DateInterval(Start, End);
            private set
            {
                Start = value.Start;
                End = value.End;
            }
        }

        private Allocation() { }

        public Allocation(Guid id, Guid tenantId, Guid rentalUnitId, string rentalUnitName, DateInterval dateInterval, decimal number)
        {
            Id = id;
            TenantId = tenantId;
            RentalUnitId = rentalUnitId;
            RentalUnitName = rentalUnitName;
            Start = dateInterval.Start;
            End = dateInterval.End;
            Number = number;
        }
    }
}