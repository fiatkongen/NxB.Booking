using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Clients.Interfaces;
using NxB.Dto.TallyWebIntegrationApi;

namespace NxB.Clients
{
    public class AccessGroupClient : NxBAdministratorClient, IAccessGroupClient
    {
        public AccessGroupClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task<AccessGroupDto> FindAccessGroup(Guid id)
        {
            var url = $"/NxB.Services.App/NxB.TallyWebIntegrationApi/accessgroup?id={id}";
            var accessGroupDto = await this.GetAsync<AccessGroupDto>(url);
            return accessGroupDto;
        }
    }
}
