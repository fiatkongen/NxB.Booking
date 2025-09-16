using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.AutomationApi
{
    public class AutomationSettingsDto
    {
        public bool IsLicensePlateAutomationEnabled { get; set; }
        public string BaseUrl { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public bool IsSetArrivedOnGateInEnabled { get; set; }
        public int NotifySetArrivedOnGateInSeconds { get; set; }
        public bool IsOutletAutomationEnabled { get; set; }
        public int OutletAutomationSynchronizeSeconds { get; set; } = 300;
        public bool UseLegacyTallyBee { get; set; }
        public List<GateSettingsDto> Gates { get; set; }
    }

    public class GateSettingsDto
    {
        public string Url { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsNotifyOnSuccessEnabled { get; set; }
        public int NotifyOnSuccessSeconds { get; set; }
        public bool IsNotifyOnErrorEnabled { get; set; }
        public int NotifyOnErrorSeconds { get; set; }
    }
}
