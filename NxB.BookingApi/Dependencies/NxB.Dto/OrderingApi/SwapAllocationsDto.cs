using System;
using System.Collections.Generic;
using System.Text;
using Munk.Utils.Object;

namespace NxB.Dto.OrderingApi
{
    public class SwapAllocationsDto
    {
        [NoEmpty]
        public Guid SubOrderId1 { get; set; }

        [NoEmpty]
        public Guid RentalUnitId1 { get; set; }

        [NoEmpty]
        public Guid SubOrderId2 { get; set; }

        [NoEmpty]
        public Guid RentalUnitId2 { get; set; }
    }
}
