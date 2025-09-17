using System;
using System.Collections.Generic;
using System.Text;
using Munk.AspNetCore;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.DocumentApi;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class BillableItem : ITenantEntity, ICreateAudit
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Guid CreateAuthorId { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now.ToEuTimeZone();
        public decimal Number { get; set; } = 1;
        public decimal Price { get; set; }
        public decimal CreditPrice { get; set; }
        public BillableItemType Type { get; set; }
        public string Text { get; set; }
        public Guid? BilledItemRef { get; set; }
        public bool Paid { get; set; }
        public DateTime? PaymentDate { get; set; }

        public Guid? OrderId { get; set; }
        public long? FriendlyOrderId { get; set; }

        public Guid? CustomerId { get; set; }
        public long? FriendlyCustomerId { get; set; }
        public Guid? MessageId { get; set; }

        public Guid? VoucherId { get; set; }
        public long? FriendlyVoucherId { get; set; }

        public bool InActive { get; private set; }
        public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.Unknown;

        private BillableItem(){}

        public BillableItem(Guid id, Guid tenantId, Guid createAuthorId, decimal number, decimal price, BillableItemType type)
        {
            Id = id;
            TenantId = tenantId;
            CreateAuthorId = createAuthorId;
            Number = number;
            Price = price;
            Type = type;
        }

        public void Activate()
        {
            InActive = false;
        }

    }
}