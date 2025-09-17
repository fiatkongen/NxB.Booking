using NxB.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class ExternalPaymentTransaction : ITenantEntity
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now.ToEuTimeZone();
        public Guid TenantId { get; set; }
        public string TransactionId { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string SaleId { get; set; }
        public Guid UserId { get; set; }
        public string Status { get; set; }
        public Guid VoucherId { get; set; }
        public string JsonResponse { get; set; }

        public ExternalPaymentTransaction(Guid id, Guid tenantId, string transactionId, string transactionType, decimal amount, string status, Guid voucherId, string jsonResponse, string saleId, Guid userId)
        {
            Id = id;
            TenantId = tenantId;
            TransactionId = transactionId;
            TransactionType = transactionType;
            Amount = amount;
            Status = status;
            JsonResponse = jsonResponse;
            SaleId = saleId;
            UserId = userId;
            VoucherId = voucherId;
        }
    }
}