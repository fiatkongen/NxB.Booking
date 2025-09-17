using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    public interface IRentalUnitRepository
    {
        void Add(RentalUnit rentalUnit);
        void Add(IEnumerable<RentalUnit> rentalUnits);
        void Delete(Guid id);
        void Update(RentalUnit rentalUnit);
        void MarkAsDeleted(Guid id);
        void MarkAsUnDeleted(Guid id);
        RentalUnit FindSingleOrDefault(Guid id);
        RentalUnit FindSingle(Guid id);
        string TryGetRentalUnitName(Guid id);
        Task<List<RentalUnit>> FindAll();
        Task<List<RentalUnit>> FindAllFromAllocationIds(IEnumerable<Guid> allocationIds);
        Task<List<RentalUnit>> FindAllIncludeDeleted();
        Task<List<RentalUnit>> FindAllOnline();
        Task<List<RentalUnit>> FindAllKiosk();
    }
}
