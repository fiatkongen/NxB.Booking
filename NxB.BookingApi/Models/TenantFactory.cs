using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AutoMapper;

namespace NxB.BookingApi.Models
{
    public class TenantFactory
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        public Tenant Create(string clientId)
        {
            return this.Create(Guid.NewGuid(), clientId);
        }

        public Tenant Create(Guid id, string clientId)
        {
            if (id == Guid.Empty) throw new ArgumentException();
            if (string.IsNullOrWhiteSpace(clientId)) throw new ArgumentException(nameof(clientId));
            var tenant = new Tenant(id, clientId);
            return tenant;
        }
    }
}