using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NxB.BookingApi.Models
{
    public interface IAllocationRepository
    {
        void Add(Allocation allocation);
        Task Add(IEnumerable<Allocation> allocations);
        Task<Allocation> FindSingle(Guid id);
        Task<List<Allocation>> FindAll();
        Task<List<Guid>> FindAllRentalUnitIds();
        Task<List<Allocation>> FindAllWithRentalUnitIds(IEnumerable<Guid> rentalUnitIds);
        Task<List<Allocation>> FindWithinInterval(DateTime start, DateTime end);
        Task<List<Allocation>> FindOccupationsWithinInterval(DateTime start, DateTime end);
        Task<List<Allocation>> FindWithinIntervalForUnit(DateTime start, DateTime end, Guid unitId);
        Task DeleteAllocationsForRentalUnitIds(IEnumerable<Guid> rentalUnitIds);
        void DeleteAllocationsForTenant(Guid tenantId);
        
        void DeleteAllocation(Guid id);
    }
}
