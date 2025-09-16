using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Allocating.Shared.Infrastructure
{
    public interface IResetAvailabilityService
    {
        Task ResetAvailability(bool skipResetAvailabilityMatrix);
    }
}
