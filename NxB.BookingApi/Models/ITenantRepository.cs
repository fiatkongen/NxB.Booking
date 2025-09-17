using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public interface ITenantRepository
    {
        void Add(Tenant tenant);
        void Delete(Guid id);
        Tenant FindSingle(Guid id);
        Tenant FindSingleOrDefault(Guid id);
        Tenant FindSingleFromClientId(string clientId);
        Tenant FindSingleFromLegacyId(string legacyId);
        Tenant FindSingleFromSubDomain(string subDomain);
        Tenant FindSingleFromKioskId(string kioskId);
        Task<List<Tenant>> FindAll();
        void Update(Tenant modifiedTenant);
        Task DeleteBookings(Guid tenantId, bool filterImported);
        Task DeleteSingleBooking(Guid tenantId, string friendlyBookingId);
        Task DeleteVouchers(Guid tenantId);
        Task DeleteMessages(Guid tenantId);
        Task DeleteCustomers(Guid tenantId);
        Task<List<Tenant>> FindAllActive();
    }
}