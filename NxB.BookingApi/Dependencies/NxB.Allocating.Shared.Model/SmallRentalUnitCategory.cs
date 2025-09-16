using System;
using System.Collections.Generic;
using System.Linq;

namespace NxB.Allocating.Shared.Model
{
    public class SmallRentalUnitCategory
    {
        public Guid RentalCategoryId { get; set; }
        public decimal Count => RentalUnitIds.Count;
        public List<Guid> RentalUnitIds { get; set; }

        public SmallRentalUnitCategory(Guid rentalCategoryId, List<Guid> rentalUnitIds)
        {
            RentalCategoryId = rentalCategoryId;
            RentalUnitIds = rentalUnitIds.Where(x => x != Guid.Empty).ToList();
        }
    }
}
