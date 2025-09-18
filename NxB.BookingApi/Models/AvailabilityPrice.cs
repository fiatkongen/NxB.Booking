using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public class AvailabilityPrice
    {
        public Guid RentalCategoryId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime IncludedEndDate { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountAmount { get; set; }
    }
}