using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Dto.AllocationApi;
using NxB.Clients.Interfaces;

namespace NxB.Clients
{
    public class GuestCategoryClient : NxBAdministratorClientWithTenantUrlLookup, IGuestCategoryClient
    {
        public GuestCategoryClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public virtual async Task<GuestCategoryDto> FindSingleOrDefault(Guid id)
        {
            var url = $"/NxB.Services.App/NxB.AllocationApi/guestcategory?id={id}";
            var guestCategoryDto = await this.GetAsync<GuestCategoryDto>(url);
            return guestCategoryDto;
        }


        public async Task<List<GuestCategoryDto>> FindAllOnlineRentalCategoriesFromTenant(Guid tenantId)
        {
            var url = $"/NxB.Services.App/NxB.AllocationApi/guestcategory/list/all/online/tenant?tenantId=" + tenantId;
            var guestCategoryDto = await this.GetAsync<List<GuestCategoryDto>>(url);
            return guestCategoryDto;
        }

        public virtual async Task<List<GuestCategoryDto>> FindAll(bool includeDeleted)
        {
            var url = $"/NxB.Services.App/NxB.AllocationApi/guestcategory/list/all?includeDeleted=" + includeDeleted;
            var guestCategoryDto = await this.GetAsync<List<GuestCategoryDto>>(url);
            return guestCategoryDto;
        }
    }

    public class GuestCategoryClientCached : GuestCategoryClient, IGuestCategoryClientCached
    {
        private readonly List<GuestCategoryDto> _cache = new();

        public GuestCategoryClientCached(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public override async Task<List<GuestCategoryDto>> FindAll(bool includeDeleted)
        {
            var articleDtos = await base.FindAll(includeDeleted);
            _cache.AddRange(articleDtos);
            return articleDtos;
        }

        public override async Task<GuestCategoryDto> FindSingleOrDefault(Guid id)
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
