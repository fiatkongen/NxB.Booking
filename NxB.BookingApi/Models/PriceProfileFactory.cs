using System;
using AutoMapper;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.PricingApi;

namespace NxB.BookingApi.Models
{
    public class PriceProfileFactory
    {
        private readonly IClaimsProvider _claimsProvider;

        public PriceProfileFactory(IClaimsProvider claimsProvider)
        {
            _claimsProvider = claimsProvider;
        }

        public PriceProfile CreatePricePriceProfile(Guid resourceId, string name, decimal? fixedPrice)
        {
            var priceProfile =
                new PriceProfile(Guid.NewGuid(), _claimsProvider.GetTenantId(), -1, -1, -1, name)
                {
                    ResourceId = resourceId,
                    FixedPrice = fixedPrice
                };
            return priceProfile;
        }
    }
}