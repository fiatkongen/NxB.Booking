using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Munk.AspNetCore;
using NxB.Domain.Common.Constants;
using NxB.Domain.Common.Model;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class PriceProfilesValidator : IPriceProfilesValidator
    {
        private readonly IPriceProfileRepository _priceProfileRepository;

        public PriceProfilesValidator(IPriceProfileRepository priceProfileRepository)
        {
            _priceProfileRepository = priceProfileRepository;
        }

        public async Task<List<Guid>> ValidatePriceProfileIds(List<Guid> priceProfileIds, Guid tenantId)
        {
            var priceProfileRepository = _priceProfileRepository.CloneWithCustomClaimsProvider(new TemporaryClaimsProvider(tenantId, AppConstants.ADMINISTRATOR_ID, "Administrator", null, null));
            var foundPriceProfiles = await priceProfileRepository.FindFromIds(priceProfileIds);
            var result = new List<Guid>();

            foreach (var priceProfileId in priceProfileIds)
            {
                if (foundPriceProfiles.None(x => x.Id == priceProfileId))
                {
                    result.Add(priceProfileId);
                }
            }
            return result;
        }
    }
}