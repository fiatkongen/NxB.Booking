using NxB.Dto.OrderingApi;

namespace NxB.BookingApi.Models
{
    public class OrderingService : IOrderingService
    {
        public Task<OrderDto> FindOrder(Guid id, Guid tenantId, bool includeIsEqualized)
        {
            throw new NotImplementedException();
        }

        public Task<OrderDto> FindOrderFromFriendlyId(long friendlyId, Guid tenantId, bool includeIsEqualized)
        {
            throw new NotImplementedException();
        }
    }
}