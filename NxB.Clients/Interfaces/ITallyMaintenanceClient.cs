using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NxB.Dto.TallyWebIntegrationApi;

namespace NxB.Clients.Interfaces
{
    public interface ITallyMaintenanceClient: IAuthorizeClient
    {
        Task<AliveMonitorDto> GetAliveMonitor(bool forceLogin = false);
        Task PerformTconMonitoring();
    }
}
