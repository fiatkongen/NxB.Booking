using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public interface ICostIntervalRepository
    {
        void Update(CostInterval costInterval);
        void UpdateType(Guid id, string type);
        void Add(CostInterval costInterval);
        void Add(IEnumerable<CostInterval> costIntervals);
        Task<CostInterval> FindSingle(Guid id);
        Task<CostInterval> FindSingleOrDefault(Guid id);
        Task<List<CostInterval>> FindAll();
        Task<List<CostInterval>> FindAllFromTenantId(Guid tenantId);
        Task<List<CostInterval>> FindAllPriceFromPriceProfileId(Guid priceProfileId);
        void DeletePermanently(CostInterval costInterval);
        void MarkAsDeleted(Guid id);
        int DeleteAll(Guid tenantId);
    }
}