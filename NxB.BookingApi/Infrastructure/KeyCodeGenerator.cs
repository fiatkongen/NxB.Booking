using System;
using System.Threading.Tasks;
using NxB.Domain.Common.Dto;
using NxB.Domain.Common.Exceptions;
using NxB.Domain.Common.Interfaces;
// TODO: Remove NxB.Remoting dependency when Service Fabric is no longer used
// using NxB.Remoting.Interfaces.TallyWebIntegrationApi;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class KeyCodeGenerator : IKeyCodeGenerator
    {
        private int KEYCODE_CIFFERS_COUNT = 6;
        private readonly ITConRepository _tconRepository;

        public KeyCodeGenerator(ITConRepository tconRepository)
        {
            _tconRepository = tconRepository;
        }

        public async Task<int> Next(ClaimsProviderDto overrideClaimsProviderDto)
        {
            int tries = 0;

            while (tries < 10)
            {
                int keyCode = GenerateRandomValidCode();
                var radioAccessCodes = await _tconRepository.CloneWithCustomClaimsProvider(overrideClaimsProviderDto.FromDto()).FindTConRadioAccessCodesWithCode(keyCode);
                if (radioAccessCodes.Count == 0) return keyCode;
                tries++;
            }

            throw new KeyCodeException("Could not allocate next keycode. tried 10 times.");
        }

        private int GenerateRandomValidCode()
        {
            int number = 0;

            for (int i = 0; i < KEYCODE_CIFFERS_COUNT; i++)
            {
                int digit = new Random().Next(1, 8);
                var multiplier = int.Parse('1' + new string('0', i));
                number += (digit * multiplier);
            }

            return number;
        }
    }
}