using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.AutomationApi
{
    public class OutletHubDto
    {
        public Guid Id { get; set; }
        public int? ExternalId { get; set; }
        public string Name { get; set; }
        public int Sort { get; set; }
    }
}
