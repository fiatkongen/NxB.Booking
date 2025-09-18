using System;
using System.Collections.Generic;

namespace NxB.Dto.LoginApi
{
    public class CredentialsDto
    {
        public Guid Id { get; }
        public string Username { get; }
        public string Login { get; }
        public string ClientId { get; }
        public string LegacyId { get; set; }
        public List<string> Roles = new();

        public CredentialsDto(string username, string login, string clientId, string legacyId, Guid id)
        {
            Username = username;
            Login = login;
            ClientId = clientId;
            LegacyId = legacyId;
            Id = id;
        }

        public void AddRole(string role)
        {
            this.Roles.Add(role);
        }
    }
}
