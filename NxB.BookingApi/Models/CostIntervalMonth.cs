using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public class CostIntervalMonth : CostInterval
    {
        public CostIntervalMonth(Guid id, DateTime startDate, DateTime endDate, int number, Decimal cost, string costType = "CostIntervalMonth") : base(id, costType)
        {
            StartDate = startDate;
            EndDate = endDate;
            Number = number;
            Cost = cost;
        }

        public override DateTime AddTimeSpan(DateTime startDate, DateTime endDate)

        {
            return startDate.AddMonths(Number).Date;
        }

        public override string GetNumbersDescription(int number)
        {
            return Number <= 1 ? "Måned" : "Måneder";
        }
    }
}