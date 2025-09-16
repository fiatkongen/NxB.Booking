using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NxB.Dto.AllocationApi
{
    public interface IAllocationApiClient
    {
        Task<List<RentalCategoryDto>> FindRentalCategories(bool includeDeleted);
        Task<List<RentalCategoryDto>> FindOnlineRentalCategoriesFromTenantId(Guid tenantId);
        Task<List<GuestCategoryDto>> FindGuestCategories(bool includeDeleted);
        Task<List<GuestCategoryDto>> FindOnlineGuestCategoriesFromTenantId(Guid tenantId);
    }
}