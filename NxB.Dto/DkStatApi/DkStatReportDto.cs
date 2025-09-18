using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Dto.DkStatApi
{
    public class DkStatReportDto
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string Xml { get; set; }
    }
}
