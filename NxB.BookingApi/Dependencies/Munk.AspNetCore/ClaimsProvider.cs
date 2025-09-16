using Microsoft.AspNetCore.Http;
using NxB.Domain.Common.Constants;
using NxB.Domain.Common.Dto;
using NxB.Domain.Common.Interfaces;
using System;
using System.Security.Authentication;
using System.Security.Claims;

namespace Munk.AspNetCore
{
    public class ClaimsProvider : IClaimsProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClaimsProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool HasClaimsContext()
        {
            return _httpContextAccessor.HttpContext != null;
        }

        public bool IsAdministrator()
        {
            return HasClaimsContext() && GetUserId() == AppConstants.ADMINISTRATOR_ID;
        }

        public Guid? GetTenantIdOrDefault()
        {
            if (_httpContextAccessor?.HttpContext == null) return null;
            try
            {
                if (!HasGetClaim(Claims.CLAIM_TENANT_ID_NAME)) {
                    return null;
                }
                var claim = GetClaim(Claims.CLAIM_TENANT_ID_NAME);
                if (claim == null) throw new AuthenticationException("Could not find claim TenantId");
                return Guid.Parse(claim.Value);
            }
            catch
            {
                return null;
            }
        }

        public Guid GetTenantId()
        {
            var claim = GetClaim(Claims.CLAIM_TENANT_ID_NAME);
            return Guid.Parse(claim.Value);
        }

        public Guid GetUserId()
        {
            var claim = GetClaim(Claims.CLAIM_USERID_NAME);
            return Guid.Parse(claim.Value);
        }

        public string GetUserLogin()
        {
            var claim = GetClaim(Claims.CLAIM_LOGIN_NAME);
            return claim.Value;
        }

        public string GetLegacyId()
        {
            var claim = GetClaim(Claims.CLAIMS_LEGACYID_NAME);
            return claim.Value;
        }

        public bool HasLegacyId()
        {
            try
            {
                var claim = GetClaim(Claims.CLAIMS_LEGACYID_NAME);
                return (claim != null);
            }
            catch
            {
                return false;
            }
        }

        private bool HasGetClaim(string claim)
        {
            if (_httpContextAccessor?.HttpContext == null) return false;
            return _httpContextAccessor.HttpContext.User.FindFirst(x => x.Type == claim) != null;
        }

        private Claim GetClaim(string claim)
        {
            if (_httpContextAccessor?.HttpContext == null) throw new AuthenticationException($"Could not find claim: {claim}. HttpContext is null");
            return _httpContextAccessor.HttpContext.User.FindFirst(x => x.Type == claim) ?? throw new AuthenticationException($"Could not find claim: {claim}"); ;
        }

        public ClaimsProviderDto ToDto()
        {
            return new ClaimsProviderDto
            {
                TenantId = GetTenantId(),
                LegacyId = GetLegacyId(),
                HasLegacyId = HasLegacyId(),
                UserId = GetUserId(),
                UserLogin = GetUserLogin()
            };
        }
    }
}
