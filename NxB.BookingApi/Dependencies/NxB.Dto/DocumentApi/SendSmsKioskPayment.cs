using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.DocumentApi
{
    public class SendSmsKioskPayment
    {
        public string Prefix { get; set; }
        public string Phone { get; set; }
        public string PaymentLink { get; set; }
        public string CustomerName { get; set; }
        public Guid TenantId { get; set; }
        public string Languages { get; set; }
        public decimal Amount { get; set; }
        public Guid SmsMessageId { get; set; }
    }
}
