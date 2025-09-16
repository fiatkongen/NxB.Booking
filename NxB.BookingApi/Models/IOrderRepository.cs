using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NxB.Domain.Common.Dto;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;

namespace NxB.BookingApi.Models
{
    public interface IOrderRepository : ICloneWithCustomClaimsProvider<IOrderRepository>
    {
        void Add(Order order);
        Task<Order> FindSingle(string id, bool includeIsEqualized);
        Task<Order> FindSingle(Guid id, bool includeIsEqualized);
        Task<Order> FindSingleOrDefault(Guid id, bool includeIsEqualized);
        Task<Order> FindSingleOrDefault(string id, bool includeIsEqualized);
        Task<Order> FindSingleFromSubOrderId(Guid subOrderId, bool includeIsEqualized);
        Task<Order> FindSingleFromExternalOrderId(string externalOrderId);
        Task<Order> FindSingleOrDefaultFromSubOrderId(Guid subOrderId, bool includeIsEqualized);
        Task<Order> FindSingleOrDefaultOrderIdFromAllocationId(Guid allocationId);
        Task<Order> FindSingleOrDefaultOrderIdFromOrderLineId(Guid orderLineId);
        Task<Order> FindSingleFromFriendlyId(long id, bool includeIsEqualized);
        Task<Order> FindSingleOrDefaultFromFriendlyId(long id, bool includeIsEqualized);
        Task<IList<Order>> FindAll(DateInterval dateInterval);
        Task DeleteImportedOrder(Guid id, DateTime importTimeStamp);
        Task<decimal> CalculateOrderTotal(Guid id);
        Task<decimal> CalculateOrderTotal(int friendlyOrderId, Guid tenantId);
        Task<decimal> CalculateAccountTotal(Guid accountId);
        void UpdateOrderNote(Guid orderId, string note, bool? noteState);
        void AppendToOrderNote(Guid orderId, string note);
        void UpdateSubOrderNote(Guid subOrderId, string note, bool? noteState);
        Task<MeterReading> GetLastMeterReading(Guid subOrderId, Guid rentalUnitId);
        Task<bool> Exists(Guid id);
        Task<Guid?> FindTenantIdFromSubOrderId(Guid subOrderId);
        Task<Guid?> FindTenantIdFromOrderId(Guid orderId);
        Task<Guid?> FindTenantIdFromExternalOrderId(string externalOrderId);

        OrderLine DeleteOrderLine(Guid id);
    }
}

