using System;
using Itenso.TimePeriod;
using Munk.Utils.Object;

namespace System
{
    [Serializable]
    public class DateInterval : ValueObject<DateInterval>, IDateInterval
    {
        public DateTime End { get; private set; }
        public DateTime Start { get; private set; }

        public DateInterval(int startYear, int startMonth, int startDate, int endYear, int endMonth, int endDate) : this(new DateTime(startYear, startMonth, startDate), new DateTime(endYear, endMonth, endDate)) { }
        public ITimeBlock TimeBlock => new TimeBlock(Start, End);
        public int Duration => TimeBlock.Duration.Days;
        public static DateInterval Eternal => new DateInterval(new DateTime(2000, 1, 1), new DateTime(2100, 1, 1));

        public DateInterval(DateTime start, DateTime end, bool allowZeroDateSpan = false)
        {
            start = start.Date;
            end = end.Date;
            Validate(start, end, allowZeroDateSpan);
            Start = start;
            End = end;
        }

        private void Validate(DateTime start, DateTime end, bool allowZeroDateSpan)
        {
            if (allowZeroDateSpan)
            {
                if (start > end)
                {
                    throw new ArgumentOutOfRangeException($"DateInterval invalid. start {start.ToDanishDate()} should be greater than end {end.ToDanishDate()}");
                }
            }
            else
            {
                if (start >= end)
                {
                    throw new ArgumentOutOfRangeException($"DateInterval invalid. start {start.ToDanishDate()} should be greater than end {end.ToDanishDate()}");
                }
            }
        }

        public DateInterval Include(DateInterval dateInterval)
        {
            var start = this.Start.Lowest(dateInterval.Start);
            var end = this.End.Highest(dateInterval.End);
            return new DateInterval(start, end);
        }

        public override string ToString()
        {
            return this.Start.ToDanishDate() + " - " + this.End.ToDanishDate();
        }

        public string ToUrlParameters()
        {
            return $"start={this.Start.ToJsonDateString()}&end={this.End.ToJsonDateString()}";
        }
    }
}
