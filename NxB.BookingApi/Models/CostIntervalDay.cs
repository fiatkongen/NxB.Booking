using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public class CostIntervalDay : CostInterval
    {
        public override string GetNumbersDescription(int number)
        {
            return number <= 1 ? "Dag" : "Dage";
        }

        public CostIntervalDay(Guid id, DateTime startDate, DateTime endDate, int number, decimal cost, string costType = "CostIntervalDay") : base(id, costType)
        {
            StartDate = startDate;
            EndDate = endDate;
            Number = number;
            Cost = cost;
        }

        public override DateTime AddTimeSpan(DateTime startDate, DateTime endDate)
        {
            return startDate.AddDays(Number).Date;
        }
    }
}