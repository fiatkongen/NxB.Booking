using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class CustomerGroup : ITenantEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TenantId { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }

        public CustomerGroup(Guid tenantId, string name)
        {
            TenantId = tenantId;
            Name = name;
        }
    }
}
