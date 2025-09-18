using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public class CostCalculationContext
    {
        public DateTime OriginalStartDate { get; set; }
        public DateTime OriginalEndDate { get; set; }

        public CostCalculationContext(DateTime originalStartDate, DateTime originalEndDate)
        {
            OriginalStartDate = originalStartDate;
            OriginalEndDate = originalEndDate;
        }
    }
}