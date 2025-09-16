using System;
using System.Collections.Generic;

namespace NxB.Dto.OrderingApi
{
    public class DiscountGroupSelectionDto
    {
        public int FilterType { get; set; }
        public List<Guid> Ids { get; set; }
    }
}
