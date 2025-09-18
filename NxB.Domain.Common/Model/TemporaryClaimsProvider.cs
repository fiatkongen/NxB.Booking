using System;
using NxB.Domain.Common.Constants;
using NxB.Domain.Common.Dto;
using NxB.Domain.Common.Interfaces;

namespace NxB.Domain.Common.Model
{
    public class TemporaryClaimsProvider : IClaimsProvider
    {
        private readonly Guid? _tenantId;
        private readonly Guid? _userId;
        private readonly string _userLogin;
        private readonly string _legacyId;
        private readonly bool? _hasLegacyId;

        public TemporaryClaimsProvider(Guid tenantId)
        {
            _tenantId = tenantId;
        }

        public TemporaryClaimsProvider(Guid? tenantId, Guid? userId, string userLogin, string legacyId, bool? hasLegacyId)
        {
            _tenantId = tenantId;
            _userId = userId;
            _userLogin = userLogin;
            _legacyId = legacyId;
            _hasLegacyId = hasLegacyId;
        }

        public static TemporaryClaimsProvider CreateNotInitialized()
        {
            return new TemporaryClaimsProvider(null, null, null, null, null);
        }

        public static TemporaryClaimsProvider CreateAdministrator(Guid tenantId)
        {
            return new TemporaryClaimsProvider(tenantId, AppConstants.ADMINISTRATOR_ID, "Administrator", null, false);
        }


        public static TemporaryClaimsProvider CreateOnline(Guid tenantId)
        {
            return new TemporaryClaimsProvider(tenantId, AppConstants.ONLINEUSER_ID, AppConstants.ONLINEUSER_NAME, null, false);
        }

        public Guid GetTenantId()
        {
            return _tenantId ?? throw new NullReferenceException();
        }

        public Guid? GetTenantIdOrDefault()
        {
            return _tenantId;
        }

        public Guid GetUserId()
        {
            return _userId ?? throw new NullReferenceException();
        }

        public string GetUserLogin()
        {
            return _userLogin ?? throw new NullReferenceException();
        }

        public string GetLegacyId()
        {
            return _legacyId ?? throw new NullReferenceException();
        }

        public bool HasLegacyId()
        {
            return _hasLegacyId ?? throw new NullReferenceException();
        }

        public ClaimsProviderDto ToDto()
        {
            throw new NotImplementedException();
        }

        public bool HasClaimsContext()
        {
            return true;
        }

        public bool IsAdministrator()
        {
            return (_userId != null && _userId == AppConstants.ADMINISTRATOR_ID);
        }
    }
}