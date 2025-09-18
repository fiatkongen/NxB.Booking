using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NxB.Dto.PricingApi;

namespace NxB.Clients.Interfaces
{
    public interface IAvailabilityPriceClient : IAuthorizeClient
    {
        Task<AvailabilityPriceDto[]> BuildOnlinePriceAvailabilityArray(DateTime start, DateTime end, Guid rentalCategoryId, Guid? tenantId = null);
        Task<AvailabilityPriceDto[]> BuildCtoutvertPriceAvailabilityArray(DateTime start, DateTime end, Guid rentalCategoryId, Guid? tenantId = null);
        Task<AvailabilityPriceDto[]> BuildKioskPriceAvailabilityArray(DateTime start, DateTime end, Guid rentalCategoryId, Guid? tenantId = null);
    }
}
