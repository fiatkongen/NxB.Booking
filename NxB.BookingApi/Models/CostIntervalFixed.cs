using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public class CostIntervalFixed : CostInterval
    {
        public override int MaxNumber
        {
            get { return 1; }
        }

        public override int MinNumber
        {
            get { return 1; }
        }

        public override string GetNumbersDescription(int number)
        {
            return "Fast";
        }

        public CostIntervalFixed(Guid id, DateTime startDate, DateTime endDate, decimal cost) : base(id, "CostIntervalFixed")
        {
            StartDate = startDate;
            EndDate = endDate;
            Cost = cost;
            Number = 1;
        }

        public override DateTime AddTimeSpan(DateTime startDate, DateTime endDate)
        {
            return this.EndDate.Date;
        }
    }
}