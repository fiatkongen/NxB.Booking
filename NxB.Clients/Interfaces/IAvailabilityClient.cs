using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NxB.Dto.AllocationApi;

namespace NxB.Clients.Interfaces
{
    public interface IAvailabilityClient : IAuthorizeClient
    {
        Task<List<AvailabilityDto>> GetRentalUnitsAvailability(DateTime start, DateTime end);
        Task<List<AvailabilityDto>> GetRentalUnitsAvailabilityFiltered(DateTime start, DateTime end, string type, Guid? filterRentalCategoryId = null);
        Task<Dictionary<string, decimal[]>> GetRentalUnitAvailabilityAsArrays(DateTime start, DateTime end, string type, Guid? filterRentalCategoryId = null);
    }
}
