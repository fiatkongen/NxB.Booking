using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Dto.AllocationApi
{
    public class GuestRentalCategoryDto
    {
        public Guid RentalCategoryId { get; set; }
        public int? MaxGuests { get; set; }
        public int? MinGuests { get; set; }
        public int? DefaultGuests { get; set; }
    }
}
