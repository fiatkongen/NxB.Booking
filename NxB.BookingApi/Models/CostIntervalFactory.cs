using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    public class CostIntervalFactory
    {
        private readonly IClaimsProvider _claimsProvider;

        public CostIntervalFactory(IClaimsProvider claimsProvider)
        {
            _claimsProvider = claimsProvider;
        }

        public CostInterval Create(string type)
        {
            var costInterval = new CostInterval(Guid.NewGuid(), type);
            costInterval.TenantId = _claimsProvider.GetTenantId();
            costInterval.CreateDate = DateTime.Now.ToEuTimeZone();
            costInterval.LastModifiedDate = DateTime.Now.ToEuTimeZone();
            costInterval.CreateAuthorId = _claimsProvider.GetUserId();
            costInterval.LastModifiedAuthorId = _claimsProvider.GetUserId();
            return costInterval;
        }

        public CostFlexInterval CreateFlex()
        {
            var costInterval = new CostFlexInterval(Guid.NewGuid());
            costInterval.TenantId = _claimsProvider.GetTenantId();
            costInterval.CreateDate = DateTime.Now.ToEuTimeZone();
            costInterval.LastModifiedDate = DateTime.Now.ToEuTimeZone();
            costInterval.CreateAuthorId = _claimsProvider.GetUserId();
            costInterval.LastModifiedAuthorId = _claimsProvider.GetUserId();
            return costInterval;
        }
    }
}