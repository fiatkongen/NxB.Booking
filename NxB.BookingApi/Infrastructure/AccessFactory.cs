using System;
using System.Threading.Tasks;
using NxB.Domain.Common.Exceptions;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.Clients;
using NxB.Remoting.Interfaces.TallyWebIntegrationApi;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class AccessFactory
    {
        private readonly IKeyCodeGenerator _keyCodeGenerator;
        private readonly IClaimsProvider _claimsProvider;
        private readonly IAccessRepository _accessRepository;

        public AccessFactory(IKeyCodeGenerator keyCodeGenerator, IClaimsProvider claimsProvider, IAccessRepository accessRepository)
        {
            _keyCodeGenerator = keyCodeGenerator;
            _claimsProvider = claimsProvider;
            _accessRepository = accessRepository;
        }

        public AccessFactory CloneWithCustomClaimsProvider(IClaimsProvider customClaimsProvider)
        {
            return new AccessFactory(_keyCodeGenerator, customClaimsProvider, _accessRepository);
        }

        public async Task<Access> CreateKeyCodeAccess()
        {
            int tries = 0;

            while (true)
            {
                var keyCode = await _keyCodeGenerator.Next(_claimsProvider.ToDto());
                if (await IsCodeClearedFromActivation(keyCode))
                {
                    var access = new Access(Guid.NewGuid(), _claimsProvider.GetTenantId(), keyCode, true);
                    access.IsKeyCode = true;
                    return access;
                }
                tries++;
                if (tries == 10) throw new KeyCodeException("Could not allocate next keycode. tried 10 times.");
            }
        }

        private async Task<bool> IsCodeClearedFromActivation(int code)
        {
            var access = await _accessRepository.FindAutoActivationAccessFromCode(code);
            return access == null;
        }

        public async Task<Access> CreateCardCodeAccess(int code)
        {
            var access = new Access(Guid.NewGuid(), _claimsProvider.GetTenantId(), code, true);
            access.IsKeyCode = false;
            return access;
        }
    }
}