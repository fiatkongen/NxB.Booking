using System;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public interface IPriceCalculator
    {
        Task<CostInformation?> CalculatePrice(Guid priceProfileId, DateTime start, DateTime end, Guid tenantId, IPriceProfileRepository priceProfileRepository, ICostIntervalRepository costIntervalRepository, bool ignoreCache = false, decimal? currentPrice = null, DateTime? currentPriceCreateDate = null);
        void ClearCaches(Guid tenantId);
    }
}