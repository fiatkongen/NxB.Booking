using NxB.BookingApi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server.Features;
using NxB.Clients.Interfaces;
// TODO: Remove Service Fabric dependency when migration is complete
// using NxB.MemCacheActor.Interfaces;

namespace NxB.BookingApi.Infrastructure
{
    public class TallyMonitor : ITallyMonitor
    {
        private static string LAST_TCON_ALIVE_MONITOR = "LastTallyAliveMonitor";
        private static string TCON_DOWN = "TconDown";
        private static string SERVER_DOWN = "ServerDown";

        private static int MAX_SERVER_DOWN_COUNT = 3;
        private static int MAX_TCON_DOWN_COUNT = 2;

        private readonly ITallyMaintenanceClient _tallyMaintenanceClient;
        private readonly IAlertClient _alertClient;

        public TallyMonitor(ITallyMaintenanceClient tallyMaintenanceClient, IAlertClient alertClient)
        {
            _tallyMaintenanceClient = tallyMaintenanceClient;
            _alertClient = alertClient;
        }

        // ITallyMonitor interface implementation
        public async Task Monitor()
        {
            await PerformTconMonitoring();
        }

        public async Task CheckStatus()
        {
            // TODO: Implement status checking logic
            await Task.CompletedTask;
        }

        public async Task PerformTconMonitoring()
        {
            // TODO: Implement caching without Service Fabric
            // var proxy = ServiceProxyHelper.CreateActorServiceProxy<IMemCacheActor>(0);
            // var lastTallyAliveDateTime = await proxy.GetCachedDateTime(LAST_TCON_ALIVE_MONITOR, null);
            DateTime? lastTallyAliveDateTime = null; // Placeholder

            try
            {
                var newTallyAliveDateTime = (await _tallyMaintenanceClient.GetAliveMonitor(true)).CreateDate;
                if (lastTallyAliveDateTime == null) //initialize
                {
                    // TODO: Implement caching without Service Fabric
                    // await proxy.SetCachedDateTime(LAST_TCON_ALIVE_MONITOR, newTallyAliveDateTime);
                    // await proxy.SetCachedInt(TCON_DOWN, 0);
                    // await proxy.SetCachedInt(SERVER_DOWN, 0);
                    return;
                }

                if (newTallyAliveDateTime > lastTallyAliveDateTime)
                {
                    // TODO: Implement caching without Service Fabric
                    // await proxy.SetCachedDateTime(LAST_TCON_ALIVE_MONITOR, newTallyAliveDateTime);
                    // await proxy.SetCachedInt(TCON_DOWN, 0);
                    // await proxy.SetCachedInt(SERVER_DOWN, 0);
                }
                else
                {
                    // TODO: Implement caching without Service Fabric
                    // var missedCount = await proxy.GetCachedInt(TCON_DOWN, 0);
                    var missedCount = 0; // Placeholder
                    if (missedCount == MAX_TCON_DOWN_COUNT)
                    {
                        await _alertClient.SendSmsToSupport("Tcon er nede. ");
                    }
                    // await proxy.SetCachedInt(TCON_DOWN, missedCount + 1);
                }

            }
            catch //server down?
            {
                // TODO: Implement caching without Service Fabric
                // var missedCount = await proxy.GetCachedInt(SERVER_DOWN, 0);
                var missedCount = 0; // Placeholder
                if (missedCount == MAX_SERVER_DOWN_COUNT)
                {
                    await _alertClient.SendSmsToSupport("Server er nede. ");
                }
                // await proxy.SetCachedInt(SERVER_DOWN, missedCount + 1);
            }
        }
    }
}