using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Domain.Common.Interfaces
{
    public interface ICounterPushUpdateService
    {
        Task TryPushUpdateMissingArrivalsCounter();
        Task TryPushUpdateMissingDeparturesCounter();
        Task TryPushUpdateMissingArrivalsDeparturesCounter();
        Task TryPushActiveTransactionsCounter(int count);
        Task TryPushActiveTransactionsCounter(Guid tenantId);
    }
}
