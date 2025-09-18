using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Domain.Common.Model
{
    public class QuickPaySettings
    {
        public string QuickPayMerchantId { get; set; }
        public string QuickPayNewMerchantId { get; set; }
        public string QuickPayMD5 { get; set; }
        public string QuickPayPrivateKey { get; set; }
        public string QuickPayApiUser { get; set; }
        public string QuickPayUserKey { get; set; }
        public bool IsQuickPayPaymentLinkAutoCaptured { get; set; } = true;
        public bool IsQuickPayPaymentLinkAutoFee { get; set; } = true;
        public bool IsQuickPayOnlineLinkAutoCaptured { get; set; } = false;
        public bool IsQuickPayOnlineLinkAutoFee { get; set; } = true;
    }
}
