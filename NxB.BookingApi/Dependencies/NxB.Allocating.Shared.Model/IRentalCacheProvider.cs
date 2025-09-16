using Microsoft.EntityFrameworkCore;

namespace NxB.Allocating.Shared.Model
{
    public interface IRentalCacheProvider
    {
        IAvailabilityCache RentalUnitsCache { get; }
        IAvailabilityCache RentalCategoriesCache { get; }
    }
}