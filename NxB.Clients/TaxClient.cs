using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Clients.Interfaces;

namespace NxB.Clients
{
    public class TaxClient : NxBAdministratorClient, ITaxClient
    {
        public TaxClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task<Dictionary<Guid, decimal>> MapTaxesFromResources(List<Guid> resourceIds)
        {
            var url = $"/NxB.Services.App/NxB.ReportingApi/tax/resource/map?resourceIds={string.Join(',' , resourceIds)}";
            var result = await this.GetAsync<Dictionary<Guid, decimal>>(url);
            return result;
        }
    }
}
