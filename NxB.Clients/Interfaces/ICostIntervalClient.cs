using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NxB.Dto.PricingApi;

namespace NxB.Clients.Interfaces
{
    public interface ICostIntervalClient : IAuthorizeClient
    {
        Task<CostIntervalDto> CreateCostInterval(CreateCostIntervalDto createCostIntervalDto);
        Task<List<CostIntervalDto>> CreateMultipleCostIntervals(List<CreateCostIntervalDto> createCostIntervalDtos);
        Task<CostIntervalDto> FindCostInterval(Guid id);
        Task<List<CostIntervalDto>> FindAllCostIntervals();
        Task<int> DeleteAllImported(Guid tenantId);
    }
}
