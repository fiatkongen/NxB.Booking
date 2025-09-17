using Microsoft.EntityFrameworkCore;

namespace NxB.BookingApi.Models
{
    public interface IRentalCacheProvider
    {
        IAvailabilityCache RentalUnitsCache { get; }
        IAvailabilityCache RentalCategoriesCache { get; }
    }
}