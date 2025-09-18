using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public class CostIntervalYear : CostInterval
    {
        public CostIntervalYear(Guid id, DateTime startDate, DateTime endDate, int number, Decimal cost) : base(id, "CostIntervalYear")
        {
            StartDate = startDate;
            EndDate = endDate;
            Number = number;
            Cost = cost;
        }

        public override string GetNumbersDescription(int number)
        {
            return "År";
        }

        public override DateTime AddTimeSpan(DateTime startDate, DateTime endDate)
        {
            return startDate.AddYears(Number).Date;
        }
    }
}