using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public class PaymentCompletedLock
    {
        public int QuickPayPaymentId { get; set; }
        public string Action { get; set; }
    }
}
