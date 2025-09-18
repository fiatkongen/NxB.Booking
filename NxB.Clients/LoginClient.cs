using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NxB.Clients.Interfaces;
using NxB.Dto.LoginApi;
using ServiceStack;

namespace NxB.Clients
{
    public class LoginClient : NxBClient, ILoginClient
    {
        public LoginClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task<Guid> CreateSession(LoginDto loginDto)
        {
            var url = $"/NxB.Services.App/NxB.LoginApi/login/session";
            var json = await this.PostAsync<string>(url, loginDto);
            var index = json.IndexOf("sessionId='");
            var sessionId = json.Substring(index + 11, 36);
            return  Guid.Parse(sessionId);
        }
    }
}
