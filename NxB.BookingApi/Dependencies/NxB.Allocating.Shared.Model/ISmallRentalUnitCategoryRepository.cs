using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NxB.Allocating.Shared.Model
{
    public interface ISmallRentalUnitCategoryRepository
    {
        Task<List<SmallRentalUnitCategory>> Find();
        Task<List<SmallRentalUnitCategory>> FindOnline(Guid? filterRentalCategoryId);
        Task<List<SmallRentalUnitCategory>> FindKiosk(Guid? filterRentalCategoryId);
        Task<List<SmallRentalUnitCategory>> FindCtoutvert(Guid? filterRentalCategoryId);
    }
}
