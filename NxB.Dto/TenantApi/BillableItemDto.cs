using System;
using NxB.Domain.Common.Enums;
using NxB.Dto.DocumentApi;

namespace NxB.Dto.TenantApi
{
    public class CreateBillableItemDto
    {
        public decimal Number { get; set; }
        public decimal Price { get; set; }
        public decimal CreditPrice { get; set; }
        public string Text { get; set; }
        public BillableItemType Type { get; set; }
        public Guid? BilledItemRef { get; set; }
        public bool AvoidDuplicateFromText { get; set; } = false;

        public Guid? OrderId { get; set; }
        public long? FriendlyOrderId { get; set; }

        public Guid? CustomerId { get; set; }
        public long? FriendlyCustomerId { get; set; }
        public Guid? MessageId { get; set; }

        public Guid? VoucherId { get; set; }
        public long? FriendlyVoucherId { get; set; }

        public bool InActive { get; set; }
        public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.Unknown;
    }

    public class BillableItemDto : CreateBillableItemDto
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public bool Paid { get; set; }
        public string TenantName { get; set; }
        public string CreateAuthorName { get; set; }
        public Guid TenantId { get; set; }
    }

    /*
 from: https://developer.suresms.com/https/https-callbackurl/
1 Delivery successful
2 Delivery failure
4 Message in queue
8 Delivery will never happen
 */

    public enum SureSmsStatus
    {
        DeliverySuccessful = 1,
        DeliveryFailure = 2,
        MessageInQueue = 4,
        DeliveryWillNeverHappen = 8
    }
}
