using System;
using System.Threading.Tasks;
using NxB.Domain.Common.Enums;
using NxB.Dto.OrderingApi;

namespace NxB.Clients.Interfaces
{
    public interface IOrderClient : IAuthorizeClient
    {
        Task<OrderDto> FindOrder(Guid id);
        Task<OrderDto> FindOrderFromSubOrderId(Guid subOrderId);
        Task<OrderDto> FindOrderFromExternalOrderId(string externalOrderId);
        Task<Guid?> FindTenantIdFromExternalOrderId(string externalOrderId);
        Task<OrderDto> FindOrder(long id);
        Task<bool> ExistsOrder(Guid id);
        Task<OrderDto> FindOrder(string orderId);
        Task<OrderDto> CreateOrder(CreateOrderDto createOrderDto);
        Task<OrderDto> CreateOnlineOrder(CartDto cartDto, string language);
        Task AppendToOrderNote(ModifyOrderNoteDto modifyOrderNoteDto);
        Task<decimal> CalculateOrderTotal(Guid id);
        Task<decimal> CalculateAccountTotal(Guid accountId);
        Task<OrderDto> FindSingleOrDefaultOrderIdFromOrderLineId(Guid orderLineId);
        Task UpdateOrderOnlineTransactionDetails(ModifyOrderOnlineTransactionDetails modifyOrderOnlineTransactionDetails);
    }
}

