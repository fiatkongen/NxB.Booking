using System;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    // TODO: Implement AccessGroupRepository interface - placeholder to fix compilation errors
    // This interface was referenced in controllers but not found in the codebase
    public interface IAccessGroupRepository
    {
        // TODO: Implement access group methods
        Task<AccessGroup> FindSingle(Guid accessGroupId);
    }
}