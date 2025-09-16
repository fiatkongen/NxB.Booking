using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NxB.BookingApi.Infrastructure;

namespace NxB.BookingApi.Models
{
    public interface ICustomerRepository 
    {
        void Add(Customer customer);
        void Add(IEnumerable<Customer> customers);
        void Delete(Guid id);
        void MarkAsDeleted(Guid id);
        Customer FindSingle(Guid id, bool includeDeleted = false);
        Customer FindSingleOrDefault(Guid id, bool includeDeleted = false);
        Customer FindSingleFromAccountId(Guid accountId, bool includeDeleted = false);
        Customer FindSingleOrDefaultFromFriendlyId(long friendlyId);
        Customer FindSingleFromAccountId(Guid accountId, Guid tenantId);
        Customer FindSingleIncludeDeleted(Guid id);
        Task<IList<Customer>> FindAll();
        Task<IList<Customer>> FindAllIncludeDeleted();
        Task<IList<Customer>> FindFromWildcard(string wildcard);
        void Update(Customer modifiedCustomer);
        int DeleteAllImported(Guid guid);
        void Undelete(Guid id);
        void UpdateCustomerNote(Guid customerId, string note, bool? noteState);
    }
}
