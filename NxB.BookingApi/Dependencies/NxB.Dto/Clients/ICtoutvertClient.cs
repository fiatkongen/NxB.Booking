using NxB.Dto.OrderingApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.Clients
{
    public interface ICtoutvertClient : IAuthorizeClient
    {
        Task PushAll(Guid tenantId);
        Task PushPriceAvailability(Guid tenantId, List<Guid> rentalCategoryIds, bool queue);
        Task<OrderDto> Book(string xml);
    }
}
