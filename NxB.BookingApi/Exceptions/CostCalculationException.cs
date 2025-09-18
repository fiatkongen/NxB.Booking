using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Exceptions
{
    public class CostCalculationException : Exception
    {
        public DateTime InvalidStartDate { get; set; }
        public DateTime InvalidEndDate { get; set; }

        public CostCalculationException(DateTime invalidStartDate, DateTime invalidEndDate) : base("Ingen prisopsætning for interval: " + invalidStartDate.ToDanishDate() + " - " + invalidEndDate.ToDanishDate())
        {
            InvalidStartDate = invalidStartDate;
            InvalidEndDate = invalidEndDate;
        }
    }

    public class NoCostCalculationsImportedException : CostCalculationException
    {
        public NoCostCalculationsImportedException(DateTime invalidStartDate, DateTime invalidEndDate) : base(invalidStartDate, invalidEndDate)
        {
            InvalidStartDate = invalidStartDate;
            InvalidEndDate = invalidEndDate;
        }
    }
}