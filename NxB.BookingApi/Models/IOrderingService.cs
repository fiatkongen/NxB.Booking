using NxB.Dto.OrderingApi;

namespace NxB.BookingApi.Models
{
    public interface IOrderingService
    {
        Task<OrderDto> FindOrder(Guid id, Guid tenantId, bool includeIsEqualized);
        Task<OrderDto> FindOrderFromFriendlyId(long friendlyId, Guid tenantId, bool includeIsEqualized);
    }
}