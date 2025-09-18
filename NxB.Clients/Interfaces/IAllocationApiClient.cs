using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NxB.Dto.AllocationApi;

namespace NxB.Clients.Interfaces
{
    public interface IAllocationApiClient
    {
        Task<List<RentalCategoryDto>> FindRentalCategories(bool includeDeleted);
        Task<List<RentalCategoryDto>> FindOnlineRentalCategoriesFromTenantId(Guid tenantId);
        Task<List<GuestCategoryDto>> FindGuestCategories(bool includeDeleted);
        Task<List<GuestCategoryDto>> FindOnlineGuestCategoriesFromTenantId(Guid tenantId);
    }
}