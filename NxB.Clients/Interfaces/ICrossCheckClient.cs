using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Clients.Interfaces
{
    public interface ICrossCheckClient: IAuthorizeClient
    {
        Task<int> AddLatestOutGateCodesToInGate(List<int> excludeCodes);
    }
}
