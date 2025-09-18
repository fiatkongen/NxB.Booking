using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NxB.Domain.Common.Interfaces;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Models
{
    public interface IPriceProfileRepository : ICloneWithCustomClaimsProvider<IPriceProfileRepository>
    {
        void Update(PriceProfile priceProfile);
        void Add(PriceProfile priceProfile);
        void Add(IEnumerable<PriceProfile> priceProfiles);
        Task<List<PriceProfile>> FindAll();
        Task<List<PriceProfile>> FindAllFromTenantId(Guid tenantId, bool includeDeleted);
        Task<List<PriceProfile>> FindAllIncludeDeleted();
        Task<List<PriceProfile>> FindFromResourceId(Guid resourceId);
        Task<List<PriceProfile>> FindFromIds(List<Guid> ids);
        void Delete(Guid id);
        void DeleteForResourceId(Guid resourceId);
        void MarkAsDeleted(Guid id);
        PriceProfile FindSingle(Guid id);
        PriceProfile FindSingleOrDefault(Guid id);
        Task<PriceProfile> FindSingleOrDefaultFromResourceId(Guid resourceId, string ppName);
    }
}