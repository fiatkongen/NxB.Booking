using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public interface IResetAvailabilityService
    {
        Task ResetAvailability(bool skipResetAvailabilityMatrix);
    }
}