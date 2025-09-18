using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.AccountingApi
{
    public class PaymentProvidersDto
    {
        public bool IsDankortEnabled { get; set; } = true;
        public bool IsVisaEnabled { get; set; } = true;
        public bool IsMasterCardEnabled { get; set; } = true;
        public bool IsAmericanExpressEnabled { get; set; } = false;
        public bool IsDinersClubEnabled { get; set; } = false;
        public bool IsJcbEnabled { get; set; } = true;
        public bool IsMaestroEnabled { get; set; } = true;
        public bool IsMobilePayEnabled { get; set; } = false;

        public string BuildQuickPayPaymentMethodsString()
        {
            var methods = new List<string>();
            if (IsDankortEnabled) { methods.Add("3d-dankort,dankort"); }
            if (IsVisaEnabled)
            {
                // methods.Add("visa");
                methods.Add("3d-visa,visa");
            }

            if (IsMasterCardEnabled)
            {
                // methods.Add("mastercard"); 
                methods.Add("3d-mastercard,mastercard,3d-mastercard-debet,mastercard-debet");
            }
            if (IsAmericanExpressEnabled) { methods.Add("american‑express"); }
            if (IsDinersClubEnabled) { methods.Add("diners"); }
            if (IsJcbEnabled) { methods.Add("3d-jcb,jcb"); }
            if (IsMaestroEnabled) { methods.Add("3d-maestro"); }
            if (IsMobilePayEnabled) { methods.Add("mobilepay"); }
            var methodsString = string.Join(',', methods);
            return methodsString;
        }
    }
}

