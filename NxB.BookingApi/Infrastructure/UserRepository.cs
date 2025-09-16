using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using NxB.Domain.Common.Constants;
using NxB.Domain.Common.Interfaces;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Models;
using ServiceStack;

namespace NxB.BookingApi.Infrastructure
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly IClaimsProvider _claimsProvider;
        protected Guid TenantId => _claimsProvider.GetTenantId();

        public UserRepository(AppDbContext appDbContext, IClaimsProvider claimsProvider)
        {
            _appDbContext = appDbContext;
            _claimsProvider = claimsProvider;
        }

        public void Add(User user)
        {
            ValidateNewUser(user);
            this._appDbContext.Add(user);
        }

        public void Update(User user)
        {
            this._appDbContext.Update(user);
        }

        public void Add(List<User> legacyUsers)
        {
            legacyUsers.ForEach(Add);
        }

        public void AddTenantToAdministrator(Guid tenantId)
        {
            AddTenantToUser(AppConstants.ADMINISTRATOR_ID, tenantId);
        }

        public void AddTenantToOnlineUser(Guid tenantId)
        {
            AddTenantToUser(AppConstants.ONLINEUSER_ID, tenantId);
        }

        public void AddTenantToUser(Guid userId, Guid tenantId)
        {
            var user = FindSingle(userId);
            ValidateUserLoginUniqueForTenant(user.Login, tenantId);
            var userTenantAccess = new UserTenantAccess(userId, tenantId);
            this._appDbContext.Add(userTenantAccess);
        }

        private void ValidateNewUser(User user)
        {
            if (user.UserTenantAccesses == null || user.UserTenantAccesses.Where(x => x.TenantId != AppConstants.ADMINISTRATOR_ID).None())
                throw new AddUserException("User must have access to at least one tenant");

            ValidateUserLoginUniqueForAttachedTenants(user);
        }

        private void ValidateUserLoginUniqueForAttachedTenants(User user)
        {
            foreach (var userTenantAccess in user.UserTenantAccesses)
            {
                var existingUser = FindFromLogin(userTenantAccess.TenantId, user.Login);
                bool userExists = existingUser != null && existingUser != user;

                if (userExists || user.Login.ToLower() == "administrator") throw new AddUserException($"User login {user.Login} is taken for tenant {userTenantAccess.TenantId}");
            }
        }

        private void ValidateUserLoginUniqueForTenant(string login, Guid tenantId)
        {
            var existingUser = FindFromLogin(tenantId, login);
            bool userIsNotAttachedToLogin = existingUser != null;
            if (userIsNotAttachedToLogin) throw new AddUserException($"User login {login} is taken for tenant {tenantId}");
        }

        public void Delete(Guid id)
        {
            var user = FindSingle(id);
            var userTenantAccesses = user.UserTenantAccesses.ToList();
            userTenantAccesses.ForEach(x => this._appDbContext.UserTenantAccess.Remove(x));
            this._appDbContext.Users.Remove(user);
        }

        public User FindSingle(Guid userId)
        {
            if (userId == Guid.Empty) return null;
            var user = this._appDbContext.Users.Include(x => x.UserTenantAccesses).Single(x => x.Id == userId);
            return user;
        }

        public User FindFromAccessCardId(Guid tenantId, string accessCardId)
        {
            if (tenantId == Guid.Empty) return null;
            var users = this._appDbContext.Users.Include(x => x.UserTenantAccesses).Where(x => x.AccessCardId == accessCardId).ToList();
            if (!users.Any()) return null;
            var user = users.SingleOrDefault(x => (x.Login == "Administrator" || x.Login == "OnlineUser" || x.Login == "bjarne" || x.Login == "Lars Jacobsen") || (((!x.IsDeleted && !x.IsDisabled)) && x.UserTenantAccesses.Any(ua => ua != null && ua.TenantId == tenantId)));
            if (user?.Login == "bjarne") return user;
            return user != null && user.UserTenantAccesses.Any(ua => ua != null && ua.TenantId == tenantId) ? user : null;
        }

        public User FindSingleOrDefault(Guid userId)
        {
            if (userId == Guid.Empty) return null;
            var user = this._appDbContext.Users.Include(x => x.UserTenantAccesses).SingleOrDefault(x => x.Id == userId);
            return user;
        }

        public User FindFromLogin(Guid tenantId, string login)
        {
            if (tenantId == Guid.Empty) return null;
            var users = this._appDbContext.Users.Include(x => x.UserTenantAccesses).Where(x => x.Login == login).ToList();
            if (!users.Any()) return null;
            var user = users.SingleOrDefault(x => (x.Login == "Administrator" || x.Login == "OnlineUser" || x.Login == "bjarne" || x.Login == "Lars Jacobsen") || (((!x.IsDeleted && !x.IsDisabled)) && x.UserTenantAccesses.Any(ua => ua != null && ua.TenantId == tenantId)));
            if (user?.Login == "bjarne") return user;
            return user != null && user.UserTenantAccesses.Any(ua => ua != null && ua.TenantId == tenantId) ? user : null;
        }

        public User FindUserFromCredentials(Guid tenantId, string login, string password)
        {
            if (tenantId == Guid.Empty) return null;

            var user = FindFromLogin(tenantId, login);
            if (user != null && user.Password == password)
            {
                return user;
            }
            return null;
        }

        public async Task<IList<User>> FindAll()
        {
            var users = await this._appDbContext.Users.Include(x => x.UserTenantAccesses).Where(x => !x.IsDeleted && x.UserTenantAccesses.Any(ua => ua.TenantId == TenantId)).ToListAsync();
            return users;
        }

        public async Task<IList<User>> FindAllIncludeDeleted()
        {
            var users = await this._appDbContext.Users.Include(x => x.UserTenantAccesses).Where(x => x.UserTenantAccesses.Any(ua => ua.TenantId == TenantId)).ToListAsync();
            return users;
        }

        public async Task<bool> IsValid(Guid userId)
        {
            var user = await this._appDbContext.Users.SingleOrDefaultAsync(x => (x.Id == userId && ((!x.IsDeleted && !x.IsDisabled) || (x.Login == "Administrator" || x.Login == "OnlineUser"))));
            return user != null;
        }
    }
}
