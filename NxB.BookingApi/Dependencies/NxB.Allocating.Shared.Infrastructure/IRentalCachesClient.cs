using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Allocating.Shared.Infrastructure
{
    public interface IRentalCachesClient
    {
        Task Initialize(DateTime start, DateTime end);
        bool IsTestClient { get; }
    }
}
