using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public class VerifoneTransactionStatusRequest
    {
        public MessageHeader MessageHeader { get; set; }
        public TransactionStatusRequest TransactionStatusRequest { get; set; }
    }

    public class TransactionStatusRequest
    {
        public MessageReference MessageReference { get; set; }
        public bool ReceiptReprintFlag { get; set; }
        public string DocumentQualifier { get; set; }
    }
}