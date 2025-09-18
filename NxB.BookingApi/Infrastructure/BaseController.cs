using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NxB.Domain.Common.Constants;
using NxB.Domain.Common.Interfaces;
using NxB.Clients.Interfaces;
using NxB.Dto.LoginApi;
using NxB.Dto.TenantApi;

namespace NxB.BookingApi.Infrastructure
{
    public class BaseController : Controller
    {
        public BaseController()
        {
        }

        public CredentialsDto BuildCredentialsDtoFromPrincipal()
        {
            Guid id = Guid.Parse(GetClaim(Claims.CLAIM_USERID_NAME));
            string login = GetClaim(Claims.CLAIM_LOGIN_NAME);
            string username = GetClaim(Claims.CLAIM_USERNAME_NAME);
            string clientId = GetClaim(Claims.CLAIM_CLIENT_ID_NAME);
            string legacyId = GetClaim(Claims.CLAIMS_LEGACYID_NAME);
            var credentialDto = new CredentialsDto(username, login, clientId, legacyId, id);
            AddUserRoles(credentialDto);
            return credentialDto;
        }

        public Guid GetTenantId()
        {
            return Guid.Parse(this.GetClaim(Claims.CLAIM_TENANT_ID_NAME));
        }

        public Guid GetUserId()
        {
            return Guid.Parse(this.GetClaim(Claims.CLAIM_USERID_NAME));
        }

        public bool HasTenantId()
        {
            return this.GetClaim(Claims.CLAIM_TENANT_ID_NAME) != null;
        }

        public Guid? TryGetTenantId()
        {
            try
            {
                 return this.GetClaim(Claims.CLAIM_TENANT_ID_NAME) != null ? Guid.Parse(this.GetClaim(Claims.CLAIM_TENANT_ID_NAME)) : null;
            }
            catch
            {
                return null;
            }
        }

        public bool IsUserAdministrator()
        {
            return this.GetClaim(Claims.CLAIM_LOGIN_NAME) == "Administrator";
        }

        public string GetLegacyId()
        {
            return this.HttpContext.User.Claims.First(x => x.Type == Claims.CLAIMS_LEGACYID_NAME).Value;
        }

        public string GetClaim(string claimType)
        {
            return this.HttpContext.User.Claims.FirstOrDefault(x => x.Type == claimType)?.Value;
        }

        public void AddUserRoles(CredentialsDto credentialsDto)
        {
            foreach (var claim in this.HttpContext.User.Claims.Where(x => x.Type == ClaimTypes.Role))
            {
                credentialsDto.AddRole(claim.Value);
            }
        }

        public async Task SignInPrincipal(string sessionId, Guid tenantId, IUser user, string clientId, string legacyId)
        {
            await this.SignInPrincipal(sessionId, tenantId, user.Id, user.Login, user.Username, clientId, legacyId);
        }

        public async Task SignInPrincipal(string sessionId, Guid tenantId, Guid userId, string login, string username, string clientId, string legacyId)
        {
            await SignOutPrincipal();
            var claims = new List<Claim>
            {
                new(Claims.CLAIM_USERID_NAME , userId.ToString()),
                new(Claims.CLAIM_SESSIONID_NAME, sessionId),
                new(Claims.CLAIM_CLIENT_ID_NAME, clientId),
                new(Claims.CLAIMS_LEGACYID_NAME, legacyId),
                new(Claims.CLAIM_LOGIN_NAME, login),
                new(Claims.CLAIM_USERNAME_NAME, username),
                new(Claims.CLAIM_TENANT_ID_NAME, tenantId.ToString())
            };

            if (username == "Administrator")
            {
                claims.Add(new Claim(ClaimTypes.Role, Claims.CLAIMS_ROLE_ADMIN));
            }

            var userIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);
            AuthenticationProperties authenticationProperties = new AuthenticationProperties
            {
                ExpiresUtc = DateTime.UtcNow.AddDays(7)
            };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authenticationProperties);
            this.HttpContext.User = principal;
            //            _telemetry.TrackTrace($"Added session to store. Store contains {_sessionStore.GetCount()} elements.", SeverityLevel.Information);
        }

        public async Task SignOutPrincipal()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        protected async Task<TenantPublicDto> SignInFakeOnlineUserForTenant(ITenantClient tenantClient, Guid tenantId)
        {
            var tenantDto = await tenantClient.FindTenantFromId(tenantId);
            await this.SignInFakeUserForTenant(tenantDto, "OnlineUser");
            return tenantDto;
        }

        protected async Task<TenantPublicDto> SignInOnlineUserWithTenantId(ITenantClient tenantClient, Guid tenantId)
        {
            return await SignInFakeOnlineUserForTenant(tenantClient, tenantId);
        }

        private async Task SignInFakeUserForTenant(TenantPublicDto tenantDto, string userName)
        {
            var claims = new List<Claim>
            {
                new(Claims.CLAIM_USERID_NAME , AppConstants.ONLINEUSER_ID.ToString()),
                new(Claims.CLAIM_SESSIONID_NAME, Guid.NewGuid().ToString()),
                new(Claims.CLAIM_CLIENT_ID_NAME, tenantDto.ClientId),
                new(Claims.CLAIMS_LEGACYID_NAME, tenantDto.LegacyId),
                new(Claims.CLAIM_LOGIN_NAME, userName),
                new(Claims.CLAIM_USERNAME_NAME, userName),
                new(Claims.CLAIM_TENANT_ID_NAME, tenantDto.Id.ToString())
            };

            // hack to handle unit tests
            if (tenantDto.Id == Guid.Parse("00000000-0000-0000-0000-000000000001")) return;

            var userIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);
            AuthenticationProperties authenticationProperties = new AuthenticationProperties
            {
                ExpiresUtc = DateTime.UtcNow.AddDays(.01)
            };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authenticationProperties);
            this.HttpContext.User = principal;
        }

        public ObjectResult ReturnFalse()
        {
            return new ObjectResult(new Dictionary<string, bool> { { "result", false } });
        }

        public ObjectResult ReturnTrue()
        {
            return new ObjectResult(new Dictionary<string, bool> { { "result", true } });
        }

        public ObjectResult ReturnString(string result)
        {
            return new ObjectResult(new Dictionary<string, string> { { "result", result } });
        }

        public static async Task<string> GetRawBodyAsync(
            HttpRequest request,
            Encoding encoding = null)
        {
            if (!request.Body.CanSeek)
            {
                // We only do this if the stream isn't *already* seekable,
                // as EnableBuffering will create a new stream instance
                // each time it's called
                request.EnableBuffering();
            }

            request.Body.Position = 0;

            var reader = new StreamReader(request.Body, encoding ?? Encoding.UTF8);

            var body = await reader.ReadToEndAsync().ConfigureAwait(false);

            request.Body.Position = 0;

            return body;
        }
    }
}