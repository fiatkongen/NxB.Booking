using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NxB.Domain.Common.Interfaces;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    [Serializable]
    public class FriendlyOrderIdProvider : IFriendlyOrderIdProvider
    {
        readonly AppDbContext _appDbContext;
        readonly IClaimsProvider _claimsProvider;

        public FriendlyOrderIdProvider(AppDbContext appDbContext, IClaimsProvider claimsProvider)
        {
            _appDbContext = appDbContext;
            _claimsProvider = claimsProvider;
        }

        public long GenerateNextFriendlyOrderId()
        {
            var order = _appDbContext.Orders.Where(x => x.TenantId == _claimsProvider.GetTenantId() && x.FriendlyId < 90000000).OrderByDescending(x => x.FriendlyId).FirstOrDefault();
            long nextId = order?.FriendlyId + 1 ?? 1;
            if (order != null && order.ImportTimeStamp != null && nextId < 10000000) nextId = 10000000;
            return nextId;
        }

    }
}
