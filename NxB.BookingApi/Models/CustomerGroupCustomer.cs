using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class CustomerGroupCustomer
    {
        public Guid CustomerId { get; set; }
        public Guid CustomerGroupId { get; set; }
        public bool IsDeleted {get; set; }

        public CustomerGroupCustomer(Guid customerId, Guid customerGroupId)
        {
            CustomerId = customerId;
            CustomerGroupId = customerGroupId;
        }
    }
}
