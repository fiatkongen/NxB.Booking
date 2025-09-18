using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NxB.Dto.AllocationApi;

namespace NxB.Clients.Interfaces
{
    public interface IRentalCategoryClient : IAuthorizeClient
    {
        Task<List<RentalCategoryDto>> FindAll(bool includeDeleted);
        Task<List<RentalCategoryDto>> FindAllFromType(string type);
        Task<List<RentalCategoryDto>> FindAllOnlineRentalCategoriesFromTenant(Guid tenantId);
        Task<List<RentalCategoryDto>> FindAllOnline();
        Task<List<RentalCategoryDto>> FindAllCtoutvert();
        Task<List<RentalCategoryDto>> FindAllKiosk();
        Task<RentalCategoryDto> FindSingleOrDefault(Guid id);
        //Task<RentalCategoryDto> FindSingleOrDefaultFromLegacyId(long legacyId);
    }

    public interface IRentalCategoryClientCached : IRentalCategoryClient { }
}
