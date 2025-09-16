using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.Clients
{
    public interface ICrossCheckClient: IAuthorizeClient
    {
        Task<int> AddLatestOutGateCodesToInGate(List<int> excludeCodes);
    }
}
