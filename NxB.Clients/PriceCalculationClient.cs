using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Clients.Interfaces;

namespace NxB.Clients
{
    public class PriceCalculationClient : NxBAdministratorClient, IPriceCalculationClient
    {
        private readonly Dictionary<(long, DateTime, DateTime), decimal> _cachedPrices = new();
        private readonly Dictionary<(Guid, DateTime, DateTime), decimal> _cachedPrices2 = new();

        public PriceCalculationClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task<decimal> CalculateLegacyPrices(string priceProfileIds, DateTime start, DateTime end)
        {
            var url = $"/NxB.Services.App/NxB.PricingApi/pricecalculation/legacy?priceProfileIds={priceProfileIds}&start={start.ToJsonDateString()}&end={end.ToJsonDateString()}";
            var result = await this.GetAsync<decimal>(url);
            return result;
        }

        public async Task<decimal?> CalculatePrice(Guid priceProfileId, DateTime start, DateTime end, Guid? tenantId)
        {
            if (_cachedPrices2.ContainsKey((priceProfileId, start, end)))
            {
                return _cachedPrices2[(priceProfileId, start, end)];
            }
            var url = $"/NxB.Services.App/NxB.PricingApi/pricecalculation?priceProfileId={priceProfileId}&start={start.ToJsonDateString()}&end={end.ToJsonDateString()}" + (tenantId != null ? $"&tenantId={tenantId}" : "");
            decimal result;

            try
            {
                result = await this.GetAsync<decimal>(url);
                _cachedPrices2[(priceProfileId, start, end)] = result;
            }
            catch
            {
                return null;
            }

            return result;
        }

        public async Task<decimal?> CalculateOnlinePriceFromLegacyTypeId(long legacyTypeId, DateTime start, DateTime end, string legacyClientId, Guid? tenantId = null)
        {
            if (_cachedPrices.ContainsKey((legacyTypeId, start, end)))
            {
                return _cachedPrices[(legacyTypeId, start, end)];
            }
            var url = $"/NxB.Services.App/NxB.PricingApi/pricecalculation/legacy/typeid?legacyTypeId={legacyTypeId}&start={start.ToJsonDateString()}&end={start.ToJsonDateString()}&tenantId={legacyClientId}&trueTenantId={tenantId}";
            decimal result;

            try
            {
                result = await this.GetAsync<decimal>(url);
                _cachedPrices[(legacyTypeId, start, end)] = result;
            }
            catch
            {
                return null;
            }

            return result;
        }
    }
}
