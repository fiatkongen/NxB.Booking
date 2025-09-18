using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Model;

namespace NxB.Domain.Common.Dto
{
    public class ClaimsProviderDto
    {
        public Guid TenantId { get; set; }
        public Guid UserId { get; set; }
        public string UserLogin { get; set; }
        public string LegacyId { get; set; }
        public bool HasLegacyId { get; set; }

        public TemporaryClaimsProvider FromDto()
        {
            return new TemporaryClaimsProvider(this.TenantId, this.UserId, this.UserLogin, this.LegacyId, this.HasLegacyId);
        }
    }
}
