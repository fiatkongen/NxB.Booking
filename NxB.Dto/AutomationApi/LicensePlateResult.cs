using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.AutomationApi
{
    public class LicensePlateResult
    {
        public bool Success { get; set; }
        public List<string> Codes { get; set; }
    }
}
