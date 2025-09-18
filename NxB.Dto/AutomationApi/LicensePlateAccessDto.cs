using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.AutomationApi
{
    public class LicensePlateAccessDto
    {
        public string Id { get; set; }
        public int CustomerId { get; set; } = 0;
        public string CustomerName { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int MaxCarsIn { get; set; }
        public List<string> LicensePlates { get; set; } = new();
        public Guid? OrderId { get; set; }
        public int? FriendlyOrderId { get; set; }
        public string RentalUnitName { get; set; }
    }

    public class ModifyLicensePlateAccessIntervalDto
    {
        public string Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int? FriendlyOrderId { get; set; }
    }
}
