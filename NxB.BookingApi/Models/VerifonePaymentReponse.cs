using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public class TransactionID_
    {
        public string TransactionID { get; set; }
        public DateTime? TimeStamp { get; set; }
    }

    public class SaleData
    {
        public string OperatorID { get; set; }
        public TransactionID_ SaleTransactionID { get; set; }
        public string TokenRequestedType { get; set; }
        public string SaleReferenceID { get; set; }
        public string SaleTerminalData { get; set; }
        public string CustomerOrderReq { get; set; }
        public string SaleToPOIData { get; set; }
        public string SaleToAcquirerData { get; set; }
        public string VF_SaleNote { get; set; }
    }

    public class POITransactionID
    {
        public string TransactionID { get; set; }
        public DateTime? TimeStamp { get; set; }
    }

    public class POIData
    {
        public POITransactionID POITransactionID { get; set; }
        public string POIReconciliationID { get; set; }
    }

    public class SensitiveCardData
    {
        public string PAN { get; set; }
        public string CardSeqNumb { get; set; }
        public string ExpiryDate { get; set; }
        public string TrackData { get; set; }
    }

    public class PaymentToken
    {
        public string TokenRequestedType { get; set; }
        public string TokenValue { get; set; }
        public DateTime? ExpiryDateTime { get; set; }
    }

    public class CardData
    {
        public object BankUserData { get; set; }
        public string PaymentBrand { get; set; }
        public string MaskedPAN { get; set; }
        public object PaymentAccountRef { get; set; }
        public object EntryMode { get; set; }
        public string CardCountryCode { get; set; }
        public object ProtectedCardData { get; set; }
        public SensitiveCardData SensitiveCardData { get; set; }
        public object AllowedProductCode { get; set; }
        public PaymentToken PaymentToken { get; set; }
        public object VF_CardholderName { get; set; }
        public Dictionary<string, string> VF_EmvTags { get; set; }
    }

    public class PaymentInstrumentData
    {
        public string PaymentInstrumentType { get; set; }
        public object ProtectedCardData { get; set; }
        public CardData CardData { get; set; }
        public string VF_AccountType { get; set; }
        public object CustomerToken { get; set; }
        public object _vf_AlternativePaymentData { get; set; }
    }

    public class AmountsResp
    {
        public string Currency { get; set; }
        public double AuthorizedAmount { get; set; }
        public double? TotalRebatesAmount { get; set; }
        public double? TotalFeesAmount { get; set; }
        public double? CashBackAmount { get; set; }
        public double? TipAmount { get; set; }
        public double? VF_FsaAuthorizedAmount { get; set; }
        public double? VF_DiffAmountDue { get; set; }
    }

    public class SignatureImage
    {
        public object ImageFormat { get; set; }
        public object ImageData { get; set; }
        public object ImageReference { get; set; }
    }

    public class CapturedSignature
    {
        public object RawSignature { get; set; }
        public SignatureImage SignatureImage { get; set; }
    }

    public class PaymentAcquirerData
    {
        public string AcquirerID { get; set; }
        public string MerchantID { get; set; }
        public string AcquirerPOIID { get; set; }
        public TransactionID_ AcquirerTransactionID { get; set; }
        public string ApprovalCode { get; set; }
        public string HostReconciliationID { get; set; }
    }

    

    public class PaymentReceipt
    {
        public string DocumentQualifier { get; set; }
        public object IntegratedPrintFlag { get; set; }
        public object RequiredSignatureFlag { get; set; }
        public OutputContent OutputContent { get; set; }
    }

    public class PaymentResponse
    {
        public class Response_
        {
            public string Result { get; set; }
            public string ErrorCondition { get; set; }
            public string AdditionalResponse { get; set; }
        }

        public Response_ Response { get; set; }
        public SaleData SaleData { get; set; }
        public POIData POIData { get; set; }

        public class PaymentResult_
        {
            public string PaymentType { get; set; }
            public PaymentInstrumentData PaymentInstrumentData { get; set; }
            public AmountsResp AmountsResp { get; set; }
            public object MerchantOverrideFlag { get; set; }
            public CapturedSignature CapturedSignature { get; set; }
            public object ProtectedSignature { get; set; }
            public bool? OnlineFlag { get; set; }
            public List<string> AuthenticationMethod { get; set; }
            public object ValidityDate { get; set; }
            public PaymentAcquirerData PaymentAcquirerData { get; set; }
        }

        public PaymentResult_ PaymentResult { get; set; }
        public PaymentReceipt PaymentReceipt { get; set; }
    }

    public class VerifonePaymentResponse
    {
        public MessageHeader MessageHeader { get; set; }
        public PaymentResponse PaymentResponse { get; set; }
    }

}