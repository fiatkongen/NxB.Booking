using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    // TODO: Implement SetupRepository interface - placeholder to fix compilation errors
    // This interface was referenced in controllers but not found in the codebase
    public interface ISetupRepository
    {
        // TODO: Implement setup period methods
        Task<List<SetupPeriod>> FindSetupPeriods();
        Task Update(SetupPeriod setupPeriod);
        Task Add(SetupPeriod setupPeriod);
        Task RemoveSetupPeriod(int no);

        // TODO: Implement setup access methods
        Task<List<SetupAccess>> FindSetupAccesses();
        Task Update(SetupAccess setupAccess);
        Task Add(SetupAccess setupAccess);
        Task RemoveSetupAccess(int no);
    }
}