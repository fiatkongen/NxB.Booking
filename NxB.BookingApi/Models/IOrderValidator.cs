using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public interface IOrderValidator
    {
        public Task ValidateOrderAndInitializeCaches(Order order);
        public Task ValidateOrderAndInitializeCaches(List<Order> orders);
    }
}
