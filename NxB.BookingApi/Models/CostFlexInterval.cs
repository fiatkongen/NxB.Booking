using AutoMapper.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    [Serializable]
    //maybe for a future implementation, where this is the only type?
    public class CostFlexInterval : CostInterval
    {
        private CostSpan _costSpan;

        public CostSpan CostSpan
        {
            get => _costSpan ??= new CostSpan(SpecificArrivalDay, SpecificArrivalDate, this.Number);
            set => _costSpan = value;
        }

        public override string GetNumbersDescription(int number)
        {
            return CostSpan.GetSpanDescription(number);
        }

        public CostFlexInterval(Guid id) : base(id, "CostFlexInterval")
        {
        }

        public CostFlexInterval(Guid id, DateTime startDate, DateTime endDate, int number, decimal cost, DayOfWeek? specificArrivalDay, int? specificArrivalDate, string costType = "CostFlexInterval") : base(id, costType)
        {
            SpecificArrivalDay = specificArrivalDay;
            SpecificArrivalDate = specificArrivalDate;
            StartDate = startDate;
            EndDate = endDate;
            Number = number;
            Cost = cost;
        }

        public CostFlexInterval Copy(DateTime startDate, DateTime endDate)
        {
            return new CostFlexInterval(Guid.NewGuid(), startDate, endDate, Number, Cost, SpecificArrivalDay, SpecificArrivalDate);
        }

        public override bool CheckIfValid(DateTime startDate, DateTime endDate, CostCalculationContext costCalculationContext)
        {
            // if (endDate.Subtract(startDate).Days != this.Number) return false;
            return true;
        }

        public override List<DateInterval> BuildMaximumDateIntervals()
        {
            return CostSpan.BuildDateIntervals(new DateInterval(StartDate, EndDate)).OrderBy(x => x.Start).ToList();
        }

        public override List<CostInterval> BuildLongestChildCostIntervals()
        {
            var dateIntervals = BuildMaximumDateIntervals();
            var longestChildCostIntervals = dateIntervals.Select(x => Copy(x.Start, x.End)).Cast<CostInterval>().ToList();
            return longestChildCostIntervals;
        }

        /// <summary>
        /// Returns the startDate + the duration of the CostInterval (necessary because of months! )
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public override DateTime AddTimeSpan(DateTime startDate, DateTime endDate)
        {
            return this.CostSpan.AddSpan(startDate);
        }
    }



    public class CostSpan
    {
        public DayOfWeek? SpecificArrivalDay;
        public int? DateOfArrival;
        public int Span = 1;

        public CostSpan(DayOfWeek? specificArrivalDay, int? dateOfArrival, int span)
        {
            SpecificArrivalDay = specificArrivalDay;
            DateOfArrival = dateOfArrival;
            Span = span;
        }

        public DateTime AddSpan(DateTime start)
        {
            return start.AddDays(Span).Date;
        }

        public string GetSpanDescription(int number)
        {
            return number <= 1 ? "Dag" : "Dage";
        }

        public List<DateInterval> BuildDateIntervals(DateInterval dateInterval)
        {
            var dateIntervals = new List<DateInterval>();
            for (int i = 0; i < Span; i++)
            {
                var currentDate = dateInterval.Start.AddDays(i);

                var currentEndDate = this.AddSpan(currentDate);

                while (currentEndDate <= dateInterval.End)
                {
                    if (this.SpecificArrivalDay == null || currentDate.DayOfWeek == this.SpecificArrivalDay)
                    {
                        dateIntervals.Add(new DateInterval(currentDate, currentEndDate));
                    }

                    currentDate = currentEndDate;
                    currentEndDate = this.AddSpan(currentEndDate);
                }
            }

            return dateIntervals;
        }
    }
}