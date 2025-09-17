using System.Collections.Generic;
using System.Threading.Tasks;
using NxB.Allocating.Shared.Infrastructure;

namespace NxB.BookingApi.Models
{
    public interface IAvailabilityMatrixRepository
    {
        void AddAndSave(AvailabilityMatrix availabilityMatrix);
        Task<AvailabilityMatrix> FindSingleOrDefault(string key);
        Task<IList<AvailabilityMatrix>> Find(IEnumerable<string> keys);
        Task<IList<AvailabilityMatrix>> FindUnseeded(IEnumerable<string> keys);
        Task<IList<AvailabilityMatrix>> FindAll();
        Task SaveChangesToAppDbContext();
        void DeleteAll();
        void ClearLocalCache();
        //Task ShrinkAllocationResources(List<string> validResourceIds);
    }
}