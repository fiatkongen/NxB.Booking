using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NxB.Dto.AllocationApi;

namespace NxB.Dto.Clients
{
    public interface IGuestCategoryClient : IAuthorizeClient
    {
        Task<GuestCategoryDto> FindSingleOrDefault(Guid id);
        Task<List<GuestCategoryDto>> FindAllOnlineRentalCategoriesFromTenant(Guid tenantId);
    }

    public interface IGuestCategoryClientCached : IGuestCategoryClient{}
}
