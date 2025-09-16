using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NxB.Allocating.Shared.Model
{
    public interface IRentalCaches : IAvailabilityCacheInitializer
    {
        Task AddAllocations(IEnumerable<Allocation> allocations);
        Task AddAllocationsAndSave(IEnumerable<Allocation> allocations);

        Task<Dictionary<string, decimal>> GetRentalCategoryAvailabilityAsCount(DateTime start, DateTime end);
        Task<Dictionary<string, decimal>> GetRentalCategoryOnlineAvailabilityAsCount(DateTime start, DateTime end, Guid? filterRentalCategoryId);
        Task<Dictionary<string, decimal>> GetRentalCategoryKioskAvailabilityAsCount(DateTime start, DateTime end, Guid? filterRentalCategoryId);

        Task<Dictionary<string, decimal[]>> GetRentalUnitAvailabilityAsArrays(DateTime start, DateTime end);
        Task<Dictionary<string, decimal>> GetRentalUnitAvailabilityAsCount(DateTime start, DateTime end);
        Task<Dictionary<string, decimal>> GetRentalUnitOnlineAvailabilityAsCount(DateTime start, DateTime end, Guid? filterRentalCategoryId = null);
        Task<Dictionary<string, decimal[]>> GetRentalUnitOnlineAvailabilityAsArrays(DateTime start, DateTime end, Guid? filterRentalCategoryId = null);
        Task<Dictionary<string, decimal>> GetRentalUnitKioskAvailabilityAsCount(DateTime start, DateTime end, Guid? filterRentalCategoryId = null);
        Task<Dictionary<string, decimal[]>> GetRentalUnitKioskAvailabilityAsArrays(DateTime start, DateTime end, Guid? filterRentalCategoryId = null);
        Task<Dictionary<string, decimal>> GetRentalUnitCtoutvertAvailabilityAsCount(DateTime start, DateTime end, Guid? filterRentalCategoryId = null);
        Task<Dictionary<string, decimal[]>> GetRentalUnitCtoutvertAvailabilityAsArrays(DateTime start, DateTime end, Guid? filterRentalCategoryId = null);
    }
}