using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Text;
using Microsoft.AspNetCore.Http;
using NxB.Domain.Common.Constants;
using NxB.Domain.Common.Interfaces;

namespace Munk.AspNetCore
{
    public class ClientIdProvider: IClientIdProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClientIdProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetClientId()
        {
            var clientIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(x => x.Type == Claims.CLAIM_CLIENT_ID_NAME);
            if (clientIdClaim == null) throw new AuthenticationException("Could not find claim ClientId");
            return clientIdClaim.Value;
        }
    }
}
