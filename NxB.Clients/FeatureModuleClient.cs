using NxB.Clients.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Dto.AutomationApi;

namespace NxB.Clients
{
    public class FeatureModuleClient : NxBAdministratorClient, IFeatureModuleClient
    {
        public static string SERVICEURL = "/NxB.Services.App/NxB.TenantApi";

        public FeatureModuleClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task<bool> IsFeatureModuleActivatedForTenant(Guid featureModuleId, Guid tenantId)
        {
            var url = $"{SERVICEURL}/featuremodule/tenant/isactive?featureModuleId={featureModuleId}&tenantId={tenantId}";
            return await this.GetAsync<bool>(url);
        }
    }
}
