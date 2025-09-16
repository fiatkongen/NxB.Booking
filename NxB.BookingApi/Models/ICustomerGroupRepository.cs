using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public interface ICustomerGroupRepository
    {
        void Add(CustomerGroup customerGroup);
        void MarkAsDeleted(Guid id);
        CustomerGroup FindSingle(Guid id);
        Task<List<CustomerGroup>> FindAll();
    }
}
