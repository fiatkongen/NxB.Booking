using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NxB.Domain.Common.Interfaces;
using NxB.BookingApi.Exceptions;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class User : IUser
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string LanguageId { get; set; }
        public string Phone { get; set; }
        public string CountryId { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsImported { get; set; }
        public string AccessCardId { get; set; }
        public string Roles { get; set; }
        public DateTime? PasswordExpirationDate { get; set; }
        public DateTime? PasswordUpdatedDate { get; set; }
        public string AvatarUrl { get; set; }

        private readonly List<UserTenantAccess> _userTenantAccesses = new();
        public IEnumerable<UserTenantAccess> UserTenantAccesses => _userTenantAccesses;

        private User() { }

        [JsonConstructor]
        private User(string username, string login, string password, string countryId)
        {
            Id = Guid.NewGuid();
            Username = username;
            Login = login;
            Password = password;
            CountryId = countryId;
        }

        public User(string username, string login, string password, Guid tenantId, string countryId) : this(username, login, password, countryId)
        {
            AddAccess(tenantId);
            this.SetPassword(password);
        }

        public void AddAccess(Guid tenantId)
        {
            _userTenantAccesses.Add(new UserTenantAccess(this, tenantId));
        }

        public void MarkAsDeleted()
        {
            this.IsDeleted = true;
        }

        public void MarkAsDisabled()
        {
            this.IsDisabled = true;
        }

        public void MarkAsEnabled()
        {
            this.IsDisabled = false;
        }

        public void ChangePassword(string oldPassword, string newPassword)
        {
            if (this.Password != oldPassword)
            {
                throw new ChangePasswordException(
                    "Den gamle adgangskode er ikke korrekt. Ændring af adgangskode kan ikke gennemføres.");
            }

            this.SetPassword(newPassword);
        }

        private void SetPassword(string password)
        {
            this.Password = password;
            this.PasswordUpdatedDate = DateTime.Now.ToEuTimeZone();
        }
    }
}
