using System;
using System.Threading.Tasks;
using NxB.Dto.LoginApi;

namespace NxB.Clients.Interfaces
{
    public interface IUserClient
    {
        Task AddTenantToAdministrator(Guid tenantId);
        Task AddTenantToOnlineUser(Guid tenantId);
        Task<UserDto> FindUserFromLogin(Guid tenantId, string login);
    }
}