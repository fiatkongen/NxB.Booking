using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public interface IUserRepository
    {
        void Add(User user);
        void Update(User user);
        void AddTenantToAdministrator(Guid tenantId);
        void AddTenantToOnlineUser(Guid tenantId);
        void AddTenantToUser(Guid userId, Guid tenantId);
        void Add(List<User> legacyUsers);
        void Delete(Guid id);
        User FindUserFromCredentials(Guid tenantId, string login, string password);
        User FindFromLogin(Guid tenantId, string login);
        User FindFromAccessCardId(Guid tenantId, string accessCardId);
        User FindSingleOrDefault(Guid id);
        User FindSingle(Guid id);
        Task<IList<User>> FindAll();
        Task<IList<User>> FindAllIncludeDeleted();
        Task<bool> IsValid(Guid userId);
    }
}
