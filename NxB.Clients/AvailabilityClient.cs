using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NxB.Clients.Interfaces;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.AllocationApi;

namespace NxB.Clients
{
    public class AvailabilityClient : NxBAdministratorClient, IAvailabilityClient
    {
        public AvailabilityClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task<List<AvailabilityDto>> GetRentalUnitsAvailability(DateTime start, DateTime end)
        {
            var url = $"/NxB.Services.App/NxB.AllocationApi/availability/rentalunits?start={start.ToJsonDateString()}&end={end.ToJsonDateString()}";
            var restResponse = await this.GetAsync<List<AvailabilityDto>>(url);
            return restResponse;
        }

        public async Task<List<AvailabilityDto>> GetRentalUnitsAvailabilityFiltered(DateTime start, DateTime end, string type, Guid? filterRentalCategoryId = null)
        {
            var url = $"/NxB.Services.App/NxB.AllocationApi/availability/rentalunits/{type}/tenant/filter?start={start.ToJsonDateString()}&end={end.ToJsonDateString()}" + (filterRentalCategoryId != null ? "&filterRentalCategoryId=" + filterRentalCategoryId.Value : "");
            var restResponse = await this.GetAsync<List<AvailabilityDto>>(url);
            return restResponse;
        }

        public async Task<Dictionary<string, decimal[]>> GetRentalUnitAvailabilityAsArrays(DateTime start, DateTime end, string type, Guid? filterRentalCategoryId = null)
        {
            var url = $"/NxB.Services.App/NxB.AllocationApi/availability/rentalunits/availabilityarrays/{type}?start={start.ToJsonDateString()}&end={end.ToJsonDateString()}" + (filterRentalCategoryId != null ? "&filterRentalCategoryId=" + filterRentalCategoryId.Value : "");
            var restResponse = await this.GetAsync<Dictionary<string, decimal[]>>(url);
            return restResponse;
        }
    }
}
