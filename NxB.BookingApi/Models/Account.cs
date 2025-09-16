using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query.Internal;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class Account : ITenantEntity, IAccountKey
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }
        public int Index { get; set; }
        public string FriendlyId { get; set; }

        public Guid CustomerId { get; set; }
        //public Customer Customer { get; set; }

        private Account() { }

        public Account(Guid id, Guid tenantId, Guid customerId, long friendlyCustomerId, string name, int index)
        {
            Id = id;
            TenantId = tenantId;
            CustomerId = customerId;
            Name = name;
            Index = index;
            FriendlyId = friendlyCustomerId + "-" + index;
        }
    }
}
