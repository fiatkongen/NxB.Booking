using System;
using System.Threading.Tasks;

namespace NxB.Clients.Interfaces
{
    public interface IPriceCalculationClient
    {
        Task<decimal> CalculateLegacyPrices(string priceProfileIds, DateTime start, DateTime end);
        Task<decimal?> CalculatePrice(Guid priceProfileId, DateTime start, DateTime end, Guid? tenantId);
        Task<decimal?> CalculateOnlinePriceFromLegacyTypeId(long legacyTypeId, DateTime start, DateTime end, string legacyClientId, Guid? tenantId = null);
    }
}
