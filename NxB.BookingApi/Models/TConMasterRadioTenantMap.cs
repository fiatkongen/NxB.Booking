using System;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    // TODO: Implement TConMasterRadioTenantMap entity - placeholder to fix compilation errors
    // This class should represent the mapping between master radios and tenants
    [Serializable]
    public class TConMasterRadioTenantMap : ITenantEntity
    {
        public Guid Id { get; private set; }
        public int TallyMasterRadioId { get; private set; }
        public Guid TenantId { get; private set; }

        // TODO: Implement proper constructor and domain logic
        public TConMasterRadioTenantMap(Guid id, int tallyMasterRadioId, Guid tenantId)
        {
            Id = id;
            TallyMasterRadioId = tallyMasterRadioId;
            TenantId = tenantId;
        }

        private TConMasterRadioTenantMap()
        {
            // EF Core constructor
        }
    }
}