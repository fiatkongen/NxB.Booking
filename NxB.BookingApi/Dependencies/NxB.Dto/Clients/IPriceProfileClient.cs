using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NxB.Dto.PricingApi;

namespace NxB.Dto.Clients
{
    public interface IPriceProfileClient : IAuthorizeClient
    {
        Task<PriceProfileDto> FindSingle(Guid id);
        Task<PriceProfileDto> FindSingleOrDefaultFromResourceId(Guid resourceId, string ppName = "standard");
        Task<List<PriceProfileDto>> FindAll(bool includeDeleted);
        Task<List<PriceProfileDto>> FindAllFromTenantId(Guid tenantId, bool includeDeleted);
        Task<PriceProfileDto> Create(CreatePriceProfileDto dto);
        Task<PriceProfileDto> Delete(Guid id);
        Task DeleteForResource(Guid resourceId);
        Task<PriceProfileDto> ModifyFixedPrice(Guid priceProfileId, decimal fixedPrice);
    }
}
