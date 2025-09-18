using NxB.Clients.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ServiceStack;

namespace NxB.Clients
{
    public class HomeSeerSetupClient : NxBAdministratorClient, IHomeSeerSetupClient
    {
        public static string SERVICEURL = "/NxB.Services.App/NxB.AutomationApi";

        public HomeSeerSetupClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task<int> SynchronizeDevices()
        {
            var url = $"{SERVICEURL}/homeseer/setup/devices/synchronize";
            var count = await this.PostAsync<int>(url, null);
            return count;
        }
    }
}
