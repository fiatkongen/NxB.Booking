using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public class UserTenantAccess
    {
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }

        private UserTenantAccess() { }

        public UserTenantAccess(Guid userId, Guid tenantId)
        {
            UserId = userId;
            TenantId = tenantId;
        }

        public UserTenantAccess(User user, Guid tenantId)
        {
            UserId = user.Id;
            TenantId = tenantId;
        }
    }
}
