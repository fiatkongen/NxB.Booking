using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    public class BillableItemFactory
    {
        private readonly IClaimsProvider _claimsProvider;

        public BillableItemFactory(IClaimsProvider claimsProvider)
        {
            _claimsProvider = claimsProvider;
        }

        public BillableItem Create(decimal number, decimal price, BillableItemType billableItemType, Guid? billedItemRef)
        {
            var billableItem = new BillableItem(Guid.NewGuid(), _claimsProvider.GetTenantId(), _claimsProvider.GetUserId(), number, price, billableItemType);
            return billableItem;
        }
    }
}