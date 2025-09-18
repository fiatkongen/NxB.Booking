using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.AspNetCore.Http;
using NxB.Clients.Interfaces;
using NxB.Dto.OrderingApi;

namespace NxB.Clients
{
    public class AllocationStateClient: NxBAdministratorClient, IAllocationStateClient
    {
        public AllocationStateClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task AddArrivalState(AddAllocationStateDto addAllocationStateDto)
        {
            var url = $"/NxB.Services.App/NxB.OrderingApi/allocationstate/arrival";
            await this.PostAsync(url, addAllocationStateDto);
        }

        public async Task<AllocationStateDto> FindSingleOrDefault(Guid subOrderId)
        {
            var url = $"/NxB.Services.App/NxB.OrderingApi/allocationstate?subOrderId={subOrderId}";
            return await this.GetAsync<AllocationStateDto>(url);
        }
    }
}
