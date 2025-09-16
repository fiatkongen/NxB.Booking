using System;
using System.Threading.Tasks;

namespace NxB.Dto.LoginApi
{
    public interface IUserClient
    {
        Task AddTenantToAdministrator(Guid tenantId);
        Task AddTenantToOnlineUser(Guid tenantId);
        Task<UserDto> FindUserFromLogin(Guid tenantId, string login);
    }
}