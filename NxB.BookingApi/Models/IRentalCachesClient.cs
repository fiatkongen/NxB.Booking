using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public interface IRentalCachesClient
    {
        Task Initialize(DateTime start, DateTime end);
        bool IsTestClient { get; }
    }
}