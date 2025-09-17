using NxB.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public class KioskFactory
    {
        private readonly IClaimsProvider _claimsProvider;

        public KioskFactory(IClaimsProvider claimsProvider)
        {
            _claimsProvider = claimsProvider;
        }

        public Kiosk Create(string hardwareSerialNo, string name)
        {
            return new Kiosk { Id = Guid.NewGuid(), TenantId = _claimsProvider.GetTenantId(), HardwareSerialNo = hardwareSerialNo, Name = name };
        }
    }
}