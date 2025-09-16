using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.BookingApi.Models
{
    public class DiscountGroupSelection
    {
        public int FilterType { get; set; } = 1;
        public List<Guid> Ids { get; set; } = new();
    }
}
