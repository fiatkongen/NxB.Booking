using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NxB.Dto.TenantApi;

namespace NxB.Clients.Interfaces
{
    public interface ITenantClient : IAuthorizeClient
    {
        Task<TenantPublicDto> FindTenant(string clientId);
        Task<TenantPublicDto> FindTenantFromId(Guid tenantId);
        Task<TenantPublicDto> FindTenantFromClientId(string clientId);
        Task<TenantPublicDto> FindTenantFromLegacyId(string legacyId);
        Task<Guid> MapToTenantId(string legacyId);
        Task<TenantPublicDto> FindCurrentTenant();
        Task<List<TenantPublicDto>> FindActiveTenants();
        Task<List<Guid>> FindTenantsIdsWithSetting(string settingsPath, bool? value);
        Task<Dictionary<Guid, (string, int)>> FindTenantsIdsOutletAutomationEnabled();
    }
}