using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.TenantApi
{
    public class OnlineBookingMasterDataDto
    {
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string OpeningHours { get; set; }
        public string OnlineIntersectionMinString { get; set;}
        public bool IsLicensePlateVisible { get; set; }
    }
}
