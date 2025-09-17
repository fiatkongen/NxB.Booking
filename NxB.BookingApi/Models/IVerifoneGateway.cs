using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NxB.Dto.TenantApi;

namespace NxB.BookingApi.Models
{
    public interface IVerifoneGateway
    {
        Task<VerifonePaymentResponse> Payment(string POIID, decimal amount, string transactionId, string saleId, string currency = "DKK");
        Task<VerifonePaymentResponse> Refund(string POIID, decimal amount, string transactionId, string saleId, string currency = "DKK");
        Task Abort(string POIID, string saleId);
        Task<VerifoneTransactionStatusResponse> GetTransactionStatus(string POIID, string saleId);
        Task<VerifoneStatusDto> GetStatus();
    }
}