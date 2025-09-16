using System;
using System.Runtime.CompilerServices;
using NxB.Domain.Common.Interfaces;

namespace NxB.Allocating.Shared.Infrastructure
{
    public class AvailabilityMatrixFactory
    {
        private readonly IClaimsProvider _claimsProvider;

        public AvailabilityMatrixFactory(IClaimsProvider claimsProvider)
        {
            _claimsProvider = claimsProvider;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public AvailabilityMatrix Create(string id, DateTime start, DateTime end)
        {
            var availabilityMatrix = new AvailabilityMatrix(id, _claimsProvider.GetTenantId(), start, end);
            return availabilityMatrix;
        }
    }
}
