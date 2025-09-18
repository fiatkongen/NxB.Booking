using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Clients.Interfaces;
using NxB.Dto.LoginApi;

namespace NxB.Clients
{
    public class UserClient : NxBAdministratorClient, IUserClient
    {
        public UserClient(IHttpContextAccessor httpContextAccessor):base(httpContextAccessor){}

        public async Task AddTenantToAdministrator(Guid tenantId)
        {
            var url = $"/NxB.Services.App/NxB.LoginApi/user/administrator/tenant?tenantId={tenantId}";
            await this.PutAsync<object>(url, null);
        }

        public async Task AddTenantToOnlineUser(Guid tenantId)
        {
            var url = $"/NxB.Services.App/NxB.LoginApi/user/onlineuser/tenant?tenantId={tenantId}";
            await this.PutAsync<object>(url, null);
        }

        public async Task<UserDto> FindUserFromLogin(Guid tenantId, string login)
        {
            var url = $"/NxB.Services.App/NxB.LoginApi/user/login?tenantId={tenantId}&login={login}";
            var userDto =  await this.GetAsync<UserDto>(url);
            return userDto;
        }
    }
}
