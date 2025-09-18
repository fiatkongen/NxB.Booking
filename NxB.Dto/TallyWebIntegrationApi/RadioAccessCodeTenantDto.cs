using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Model;

namespace NxB.Dto.TallyWebIntegrationApi
{
    public class RadioAccessCodeTenantDto
    {
        public Guid AccessId { get; set; }
        public uint Code { get; set; }
        public bool IsKeyCode { get; set; }
        public Guid TenantId { get; set; }
        public DateTime? ActivationDate { get; set; }
        public AccessibleItems AccessibleItems { get; set; }

        public RadioAccessCodeTenantDto() { }

        public RadioAccessCodeTenantDto(Guid accessId, bool isKeyCode, uint code, Guid tenantId, DateTime? activationDate = null)
        {
            IsKeyCode = isKeyCode;
            AccessId = accessId;
            Code = code;
            TenantId = tenantId;
            ActivationDate = activationDate;
        }
    }
}
