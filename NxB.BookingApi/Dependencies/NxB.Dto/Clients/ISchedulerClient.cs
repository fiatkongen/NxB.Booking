using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.Clients
{
    public interface ISchedulerClient
    {
        Task<List<int>> DeactivateOneOffAccesses();
        Task<int> ActivateArrivedAccesses();
    }
}
