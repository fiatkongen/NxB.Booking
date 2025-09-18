using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NxB.Dto.OrderingApi;

namespace NxB.Clients.Interfaces
{
    public interface IAccessClient : IAuthorizeClient 
    {
        Task<AccessDto> FindAccess(Guid id);
        Task<List<AccessDto>> FindAccessesForOrder(Guid orderId, bool? isKeyCode = null);
        Task<AccessDto> CreateAccessToAccessibleItems(CreateOrModifyAccessFromAccessibleItemsDto dto);
    }
}
