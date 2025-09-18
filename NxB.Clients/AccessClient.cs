using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Clients.Interfaces;
using NxB.Dto.OrderingApi;

namespace NxB.Clients
{
    public class AccessClient : NxBAdministratorClientWithTenantUrlLookup, IAccessClient
    {
        public AccessClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task<AccessDto> FindAccess(Guid id)
        {
            var url = $"/NxB.Services.App/NxB.OrderingApi/access?id={id}";
            var accessDtos = await this.GetAsync<AccessDto>(url);
            return accessDtos;
        }

        public async Task<List<AccessDto>> FindAccessesForOrder(Guid orderId, bool? isKeyCode = null)
        {
            var url = $"/NxB.Services.App/NxB.OrderingApi/access/order/list?orderId={orderId}&isKeyCode={isKeyCode}";
            var accessDtos = await this.GetAsync<List<AccessDto>>(url);
            return accessDtos;
        }

        public async Task<AccessDto> CreateAccessToAccessibleItems(CreateOrModifyAccessFromAccessibleItemsDto dto)
        {
            var url = $"/NxB.Services.App/NxB.OrderingApi/access/accessibleitems";
            var accessDto = await this.PostAsync<AccessDto>(url, dto);
            return accessDto;
        }
    }
}
