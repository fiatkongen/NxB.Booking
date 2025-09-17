using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public class MessageHeader_
    {
        public string MessageClass { get; set; }
        public string MessageCategory { get; set; }
        public string MessageType { get; set; }
        public string ServiceID { get; set; }
        public string SaleID { get; set; }
        public string POIID { get; set; }
    }

    public class TransactionIDPaymentRequest
    {
        public string TransactionID { get; set; }
        public string TimeStamp { get; set; }
    }

    public class SaleDataPaymentRequest
    {
        public string OperatorID { get; set; }
        public TransactionIDPaymentRequest SaleTransactionID { get; set; }
        public List<string> CustomerOrderReq { get; set; }
        public string SaleToAcquirerData { get; set; }
    }

    public class AmountsReq
    {
        public string Currency { get; set; }
        public decimal RequestedAmount { get; set; }
    }

    public class PaymentTransaction
    {
        public AmountsReq AmountsReq { get; set; }
    }

    public class PaymentData
    {
        public string PaymentType { get; set; }
        public bool SplitPaymentFlag { get; set; }
    }

    public class PaymentRequest
    {
        public SaleDataPaymentRequest SaleData { get; set; }
        public PaymentTransaction PaymentTransaction { get; set; }
        public PaymentData PaymentData { get; set; }
    }

    public class VerifonePaymentRequest
    {
        public MessageHeader MessageHeader { get; set; }
        public PaymentRequest PaymentRequest { get; set; }
    }

}