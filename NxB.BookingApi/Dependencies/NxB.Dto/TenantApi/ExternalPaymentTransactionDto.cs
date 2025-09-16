using NxB.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.TenantApi
{
    public class ExternalPaymentTransactionDto
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public Guid TenantId { get; set; }
        public string TransactionId { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public Guid VoucherId { get; set; }
        public int FriendlyVoucherId { get; set; }
        public long FriendlyOrderId { get; set; }
        public VoucherType VoucherType { get; set; }
        public string SaleId { get; set; }
        public Guid UserId { get; set; }
    }
}
