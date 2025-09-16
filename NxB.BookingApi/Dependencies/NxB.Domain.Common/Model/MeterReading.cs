using System;

namespace NxB.Domain.Common.Model
{
    public class MeterReading
    {
        public decimal Reading { get; set; }
        public DateTime Date { get; set; }

        public MeterReading(decimal reading, DateTime date)
        {
            Reading = reading;
            Date = date;
        }
    }
}
