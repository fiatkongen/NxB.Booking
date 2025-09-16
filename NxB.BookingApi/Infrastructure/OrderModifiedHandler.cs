using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NxB.Domain.Common.Enums;
using NxB.Dto.OrderingApi;
using NxB.MemCacheActor.Interfaces;

namespace NxB.BookingApi.Infrastructure
{
    public class OrderModifiedHandler : IOrderingActorEvents
    {
        private readonly IServiceProvider _serviceProvider;

        public OrderModifiedHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void OrderModified(Guid tenantId, OrderDto originalOrderDto, List<SubOrderDto> modifiedSubOrderDto, List<SubOrderDto> createdSubOrderDtos)
        {
//            await _ctoutvertPriceAvailabilityClient.AuthorizeClient(tenantId);
  //          await _memCacheActor.TryPublishOrderModifiedOrCreated()
//            _ctoutvertPriceAvailabilityClient.TryPushPriceAvailabilityFireAndForget(new rentalCategory.Id, true);
        }

        public void OrderCreated(Guid tenantId, OrderDto orderDto)
        {
            // throw new NotImplementedException();
        }

        public void SubOrderArrivalStateChanged(Guid tenantId, Guid subOrderId, ArrivalStatus orgArrivalState, ArrivalStatus newArrivalState)
        {
        }

        public void SubOrderDepartureStateChanged(Guid tenantId, Guid subOrderId, DepartureStatus orgDepartureState,
            DepartureStatus newDepartureState)
        {
        }

        public void SubOrderCancelled(Guid tenantId, Guid subOrderId)
        {
        }
    }
}