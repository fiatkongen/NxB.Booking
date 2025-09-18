using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NxB.Domain.Common.Interfaces;
using NxB.Clients.Interfaces;
using NxB.Dto.TenantApi;
using ServiceStack.Pcl;
using HttpUtility = System.Web.HttpUtility;

namespace NxB.Clients
{
    //https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
    public class TenantClient : NxBAdministratorClient, ITenantClient
    {
        public static string SERVICEURL = "/NxB.Services.App/NxB.TenantApi";

        public TenantClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public Task<TenantPublicDto> FindTenant(string clientId)
        {
            var isGuid = Guid.TryParse(clientId, out var tenantGuid);
            if (isGuid)
            {
                return FindTenantFromId(tenantGuid);
            }
            else
            {
                return FindTenantFromClientId(clientId);
            }
        }

        public async Task<TenantPublicDto> FindTenantFromId(Guid tenantId)
        {
            var url = $"{SERVICEURL}/tenant?id=" + tenantId;
            var restResponse = await this.GetAsync<TenantPublicDto>(url);
            return restResponse;
        }

        public async Task<TenantPublicDto> FindTenantFromClientId(string clientId)
        {
            var url = $"{SERVICEURL}/tenant/query/clientid?id=" + HttpUtility.UrlEncode(clientId);
            var restResponse = await this.GetAsync<TenantPublicDto>(url);
            return restResponse;
        }

        public async Task<TenantPublicDto> FindTenantFromLegacyId(string legacyId)
        {
            var url = $"{SERVICEURL}/tenant/query/legacyid?id=" + HttpUtility.UrlEncode(legacyId);
            var restResponse = await this.GetAsync<TenantPublicDto>(url);
            return restResponse;
        }

        public async Task<Guid> MapToTenantId(string legacyId)
        {
            var tenant = await FindTenantFromLegacyId(legacyId);
            return tenant.Id;
        }

        public async Task<TenantPublicDto> FindCurrentTenant()
        {
            var url = $"{SERVICEURL}/tenant/current";
            var restResponse = await this.GetAsync<TenantPublicDto>(url);
            return restResponse;
        }

        public async Task<List<TenantPublicDto>> FindActiveTenants()
        {
            var url = $"{SERVICEURL}/tenant/list/all/active";
            var restResponse = await this.GetAsync<List<TenantPublicDto>>(url);
            return restResponse;
        }

        public async Task<List<Guid>> FindTenantsIdsWithSetting(string settingsPath, bool? value)
        {
            var url = $"{SERVICEURL}/tenant/settings/tenantids/all?settingsPath={settingsPath}{(value.HasValue ? $"&value={value}" : "")}";
            var restResponse = await this.GetAsync<List<Guid>>(url);
            return restResponse;
        }

        public async Task<Dictionary<Guid, (string, int)>> FindTenantsIdsOutletAutomationEnabled()
        {
            var url = $"{SERVICEURL}/tenant/settings/tenantids/all/outletautomationenabled";
            var restResponse = await this.GetAsync<Dictionary<Guid, (string, int)>>(url);
            return restResponse;
        }
    }
}
