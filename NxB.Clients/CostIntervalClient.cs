using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Clients.Interfaces;
using NxB.Dto.PricingApi;

namespace NxB.Clients
{
    public class CostIntervalClient : NxBAdministratorClient, ICostIntervalClient
    {
        public static string SERVICEURL = "/NxB.Services.App/NxB.PricingApi";

        public CostIntervalClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task<CostIntervalDto> CreateCostInterval(CreateCostIntervalDto createCostIntervalDto)
        {
            var url = $"{SERVICEURL}/costinterval";
            return await this.PostAsync<CostIntervalDto>(url, createCostIntervalDto);

        }

        public async Task<List<CostIntervalDto>> CreateMultipleCostIntervals(List<CreateCostIntervalDto> createCostIntervalDtos)
        {
            var url = $"{SERVICEURL}/costinterval/list/all";
            return await this.PostAsync<List<CostIntervalDto>>(url, createCostIntervalDtos);

        }

        public async Task<CostIntervalDto> FindCostInterval(Guid id)
        {
            var url = $"{SERVICEURL}/costinterval?id={id}";
            return await this.GetAsync<CostIntervalDto>(url);
        }

        public async Task<List<CostIntervalDto>> FindAllCostIntervals()
        {
            var url = $"{SERVICEURL}/costinterval";
            return await this.GetAsync<List<CostIntervalDto>>(url);
        }

        public async Task<int> DeleteAllImported(Guid tenantId)
        {
            var url = $"{SERVICEURL}/costinterval/list/all?tenantId="+ tenantId;
            return await this.DeleteAsync<int>(url);
        }
    }
}
