using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    public interface IAccountRepository : ICloneWithCustomClaimsProvider<IAccountRepository>
    {
        void Add(Account account);
        void MarkAsDeleted(Guid id);
        Account FindSingle(Guid id);
        Account FindSingleOrDefault(Guid id);
        Task<IList<Account>> FindAll();
        Task<IList<Account>> FindAllForCustomer(Guid id);
        Task<IList<Account>> FindAllForCustomerIncludeDeleted(Guid id);
        //Task<Decimal> CalculateNotInvoicedForOrder(Guid orderId);
        //Task<Decimal> CalculateNotInvoicedForAccount(Guid accountId);
    }
}

