using System;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    // TODO: Implement TConMasterRadioTenantMapRepository interface - placeholder to fix compilation errors
    // This interface was referenced in controllers but not found in the codebase
    public interface ITConMasterRadioTenantMapRepository : IMasterRadioIdProvider
    {
        // TODO: Implement master radio tenant mapping methods
        void Add(TConMasterRadioTenantMap conMasterRadioTenantMap);
        void Delete(Guid id);
        TConMasterRadioTenantMap FindAllMasterRadioMappings();
    }
}