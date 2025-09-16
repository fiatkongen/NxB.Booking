using NxB.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using NxB.Dto.Clients;

namespace NxB.BookingApi.Models
{
    public interface IAllocationStateRepository : ICloneWithCustomClaimsProvider<IAllocationStateRepository>
    {
        void Add(AllocationState allocationState);
        AllocationState FindSingle(Guid subOrderId);
        AllocationState FindSingleOrDefault(Guid subOrderId);
        AllocationState FindSingleOrDefaultNotCommitted(Guid subOrderId);
        void Update(AllocationState allocationState);
    }
}
