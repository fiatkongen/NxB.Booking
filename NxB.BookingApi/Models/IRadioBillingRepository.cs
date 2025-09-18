using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    // TODO: Implement RadioBillingRepository interface - placeholder to fix compilation errors
    // This interface was referenced in controllers but not found in the codebase
    public interface IRadioBillingRepository
    {
        // TODO: Implement radio billing methods
        Task<List<RadioBilling>> FindAll();
        Task<RadioBilling> FindSingle(Guid id);
        Task Add(RadioBilling radioBilling);
        Task Update(RadioBilling radioBilling);
        Task Delete(Guid id);
    }
}