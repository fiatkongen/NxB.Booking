using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public class CostInformationDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Number { get; set; }
        public string Text { get; set; }
        public string CostFormatted { get; set; }
        public string StartDateFormatted { get; set; }
        public string EndDateFormatted { get; set; }
    }
}