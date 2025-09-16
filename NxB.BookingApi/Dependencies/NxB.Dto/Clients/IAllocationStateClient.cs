using NxB.Dto.OrderingApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.Clients
{
    public interface IAllocationStateClient : IAuthorizeClient
    {
        Task AddArrivalState(AddAllocationStateDto addAllocationStateDto);
        Task<AllocationStateDto> FindSingleOrDefault(Guid subOrderId);
    }
}
