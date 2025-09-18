using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.AllocationApi;
using NxB.Clients.Interfaces;

namespace NxB.Clients
{
    public class RentalCategoryClient : NxBAdministratorClient, IRentalCategoryClient
    {
        public RentalCategoryClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public virtual async Task<List<RentalCategoryDto>> FindAll(bool includeDeleted)
        {
            var url = $"/NxB.Services.App/NxB.AllocationApi/rentalcategory/list/all?includeDeleted=" + includeDeleted;
            var rentalCategories = await this.GetAsync<List<RentalCategoryDto>>(url);
            return rentalCategories;
        }

        public virtual async Task<List<RentalCategoryDto>> FindAllOnlineRentalCategoriesFromTenant(Guid tenantId)
        {
            var url = $"/NxB.Services.App/NxB.AllocationApi/rentalcategory/list/all/online/tenant?tenantId=" + tenantId;
            var rentalCategories = await this.GetAsync<List<RentalCategoryDto>>(url);
            return rentalCategories;
        }

        public async Task<List<RentalCategoryDto>> FindAllFromType(string type)
        {
            var url = $"/NxB.Services.App/NxB.AllocationApi/rentalcategory/list/all/{type}";
            var rentalCategories = await this.GetAsync<List<RentalCategoryDto>>(url);
            return rentalCategories;
        }

        public Task<List<RentalCategoryDto>> FindAllOnline()
        {
            return FindAllFromType("online");
        }

        public Task<List<RentalCategoryDto>> FindAllCtoutvert()
        {
            return FindAllFromType("ctoutvert");
        }

        public Task<List<RentalCategoryDto>> FindAllKiosk()
        {
            return FindAllFromType("kiosk");
        }

        public virtual async Task<RentalCategoryDto> FindSingleOrDefault(Guid id)
        {
            var url = $"/NxB.Services.App/NxB.AllocationApi/rentalcategory?id=" + id;
            var rentalCategoryDto = await this.GetAsync<RentalCategoryDto>(url);
            return rentalCategoryDto;
        }

    }

    public class RentalCategoryClientCached : RentalCategoryClient, IRentalCategoryClientCached
    {
        private readonly List<RentalCategoryDto> _cache = new();

        public RentalCategoryClientCached(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public override async Task<List<RentalCategoryDto>> FindAll(bool includeDeleted)
        {
            var rentalCategories =  await base.FindAll(includeDeleted);
            _cache.AddRange(rentalCategories);
            return rentalCategories;
        }

        public override async Task<RentalCategoryDto> FindSingleOrDefault(Guid id)
        {
            var item = _cache.SingleOrDefault(x => x.Id == id);
            if (item == null)
            {
                item = await base.FindSingleOrDefault(id);
                if (item != null)
                {
                    _cache.Add(item);
                }
            }
            return item;
        }

    }
}
