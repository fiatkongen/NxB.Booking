using System;
using System.Collections.Generic;
using Munk.Utils.Object;
using NxB.Domain.Common.Enums;

namespace NxB.Dto.OrderingApi
{
    public class AllocationStateDto
    {
        public Guid SubOrderId { get; set; }

        public ArrivalStatus? ArrivalStatus { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public string ArrivalText { get; set; }

        public DepartureStatus? DepartureStatus { get; set; }
        public DateTime? DepartureTime { get; set; }
        public string DepartureText { get; set; }

        public List<AllocationStateLogDto> ArrivalStateLogs { get; set; }
        public List<AllocationStateLogDto> DepartureStateLogs { get; set; }
    }

    public class AddAllocationStateDto
    {
        [NoEmpty]
        public Guid SubOrderId { get; set; }
        public AllocationStatus? Status { get; set; }
        public DateTime? CustomTime { get; set; }
        public string Text { get; set; }
    }

    public class AllocationStateLogDto
    {
        public DateTime CreateDate { get; set; }
        public Guid CreateAuthorId { get; set; }
        public AllocationStatus? Status { get; set; } 
        public DateTime? CustomTime { get; set; }
        public string Text { get; set; } 
    }
}
