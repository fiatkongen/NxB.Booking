using System;
using NxB.Domain.Common.Dto;

namespace NxB.Domain.Common.Interfaces
{
    public interface IClaimsProvider
    {
        Guid GetTenantId();
        Guid? GetTenantIdOrDefault();
        Guid GetUserId();
        string GetUserLogin();
        string GetLegacyId();
        bool HasLegacyId();

        ClaimsProviderDto ToDto();
        bool HasClaimsContext();
        bool IsAdministrator();
    }
}