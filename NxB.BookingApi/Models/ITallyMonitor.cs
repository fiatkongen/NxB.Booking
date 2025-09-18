using System;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    // TODO: Implement TallyMonitor interface - placeholder to fix compilation errors
    // This interface was referenced in controllers but not found in the codebase
    public interface ITallyMonitor
    {
        // TODO: Implement tally monitoring methods
        Task Monitor();
        Task CheckStatus();
    }
}