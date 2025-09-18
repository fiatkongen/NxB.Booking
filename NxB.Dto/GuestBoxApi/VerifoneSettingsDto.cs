using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.GuestBoxApi
{
    public class VerifoneSettingsDto
    {
        public bool IsEnabled { get; set; }
        public string AuthHeader { get; set; }
        public string XSite { get; set; }

    }
}
