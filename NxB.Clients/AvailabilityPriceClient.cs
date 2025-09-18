using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Clients.Interfaces;
using NxB.Dto.PricingApi;

namespace NxB.Clients
{
    public class AvailabilityPriceClient : NxBAdministratorClient, IAvailabilityPriceClient
    {
        public static string SERVICEURL = "/NxB.Services.App/NxB.PricingApi";

        public AvailabilityPriceClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public Task<AvailabilityPriceDto[]> BuildCtoutvertPriceAvailabilityArray(DateTime start, DateTime end, Guid rentalCategoryId, Guid? tenantId = null)
        {
            return BuildPriceAvailabilityArray(start, end, rentalCategoryId, "ctoutvert", tenantId);
        }

        public Task<AvailabilityPriceDto[]> BuildKioskPriceAvailabilityArray(DateTime start, DateTime end, Guid rentalCategoryId, Guid? tenantId = null)
        {
            return BuildPriceAvailabilityArray(start, end, rentalCategoryId, "kiosk", tenantId);
        }

        public Task<AvailabilityPriceDto[]> BuildOnlinePriceAvailabilityArray(DateTime start, DateTime end, Guid rentalCategoryId, Guid? tenantId = null)
        {
            return BuildPriceAvailabilityArray(start, end, rentalCategoryId, "online", tenantId);
        }

        private async Task<AvailabilityPriceDto[]> BuildPriceAvailabilityArray(DateTime start, DateTime end, Guid rentalCategoryId, string type, Guid? tenantId = null)
        {
            var url = $"{SERVICEURL}/availabilityprice/arrays?start={start.ToJsonDateString()}&end={end.ToJsonDateString()}&rentalCategoryId={rentalCategoryId}&type={type}{(tenantId != null ? "&tenantId=" + tenantId : "")}";
            var restResponse = await this.GetAsync<AvailabilityPriceDto[]>(url);
            return restResponse;
        }
    }
}
