using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    public class AccessGroupFactory
    {
        private readonly IClaimsProvider _claimsProvider;

        public AccessGroupFactory(IClaimsProvider claimsProvider)
        {
            _claimsProvider = claimsProvider;
        }

        public AccessGroup Create()
        {
            var accessGroup = new AccessGroup(Guid.NewGuid(), _claimsProvider.GetTenantId());
            return accessGroup;
        }
    }
}
