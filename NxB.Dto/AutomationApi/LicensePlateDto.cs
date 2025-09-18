using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.AutomationApi
{
    public class LicensePlateDto
    {
        public string LicensePlate { get; set; }
        public bool IgnoreSchedule { get; set; }
        public string CustomerName { get; set; }
    }
}
