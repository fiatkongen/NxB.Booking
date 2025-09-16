using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public interface IPaymentService
    {
        Payment Credit(Guid id);
    }
}
