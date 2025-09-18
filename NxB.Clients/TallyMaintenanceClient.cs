using NxB.Clients.Interfaces;
using NxB.Dto.TallyWebIntegrationApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Domain.Common.Interfaces;

namespace NxB.Clients
{
    public class TallyMaintenanceClient : NxBAdministratorClient, ITallyMaintenanceClient
    {
        public async Task<AliveMonitorDto> GetAliveMonitor(bool forceLogin = false)
        {
            if (forceLogin) await this.AuthorizeClient();

            return await Call(async () =>
            {
                var url = $"/NxB.Services.App/NxB.TallyWebIntegrationApi/maintenance/alivemonitor";
                return await this.GetAsync<AliveMonitorDto>(url);
            });
        }

        public async Task PerformTconMonitoring()
        {
            await Call(async () =>
            {
                var url = $"/NxB.Services.App/NxB.TallyWebIntegrationApi/maintenance/performtconmonitoring";
                await this.GetAsync(url);
            });
        }

        public TallyMaintenanceClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }
    }
}
