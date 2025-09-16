using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public interface ICustomerTestDataImporter
    {
        List<Customer> BuildCustomersTestData(Guid destinationTenantId);
    }
}
