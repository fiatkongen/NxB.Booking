using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Domain.Common.Interfaces;
using NxB.Clients.Interfaces;

namespace NxB.Clients
{
    public class CrossCheckClient : NxBAdministratorClient, ICrossCheckClient
    {

        public async Task<int> AddLatestOutGateCodesToInGate(List<int> excludeCodes)
        {
            return await Call(async () =>
            {
                var url = $"/NxB.Services.App/NxB.TallyWebIntegrationApi/crosscheck/addlatestoutcodestoingate";
                return await this.PostAsync<int>(url, excludeCodes);
            });
        }

        public CrossCheckClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }
    }
}
