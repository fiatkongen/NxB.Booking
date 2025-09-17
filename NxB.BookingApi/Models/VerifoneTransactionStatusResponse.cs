using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    using System;
    using System.Collections.Generic;

    public class TransactionStatusResponse
    {
        public ResponseDetail Response { get; set; }
        public MessageReference MessageReference { get; set; }
        public RepeatedMessageResponse RepeatedMessageResponse { get; set; }
    }

    public class ResponseDetail
    {
        public string Result { get; set; }
        public object ErrorCondition { get; set; }
        public string AdditionalResponse { get; set; }
    }

    public class RepeatedMessageResponse
    {
        public MessageHeader MessageHeader { get; set; }
        public RepeatedResponseMessageBody RepeatedResponseMessageBody { get; set; }
    }

    public class RepeatedResponseMessageBody
    {
        public PaymentResponse PaymentResponse { get; set; }
        public ReversalResponse ReversalResponse { get; set; }
    }

    public class ReversalResponse
    {
        // Define properties for ReversalResponse if needed
    }

    public class VerifoneTransactionStatusResponse
    {
        public MessageHeader MessageHeader { get; set; }
        public TransactionStatusResponse TransactionStatusResponse { get; set; }
    }
}