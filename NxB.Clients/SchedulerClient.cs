using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Domain.Common.Interfaces;
using NxB.Clients.Interfaces;
using NxB.Dto.TenantApi;
using ServiceStack;

namespace NxB.Clients
{
    public class SchedulerClient : NxBAdministratorClient, ISchedulerClient
    {
        public async Task<List<int>> DeactivateOneOffAccesses()
        {
            return await Call(async () =>
            {
                var url = "/NxB.Services.App/NxB.OrderingApi/accessschedule/oneoffaccess/deactivate";
                return await this.PostAsync<List<int>>(url, null);
            });
        }

        public async Task<int> ActivateArrivedAccesses()
        {
            return await Call(async () =>
            {
                var url = "/NxB.Services.App/NxB.OrderingApi/accessschedule/accessesarrived/activate";
                return await this.PostAsync<int>(url, null);
            });
        }

        public SchedulerClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }
    }
}
