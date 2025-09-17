using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using NxB.Domain.Common.Constants;
using NxB.Domain.Common.Model;
using NxB.Dto.OrderingApi;
using NxB.Dto.TenantApi;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class OrderingService : IOrderingService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public OrderingService(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<OrderDto> FindOrder(Guid id, Guid tenantId, bool includeIsEqualized)
        {
            var orderRepository = _orderRepository.CloneWithCustomClaimsProvider(new TemporaryClaimsProvider(tenantId, AppConstants.ADMINISTRATOR_ID, "Administrator", null, null));

            var order = await orderRepository.FindSingleOrDefault(id, includeIsEqualized);
            if (order == null) return null;
            var orderDto = _mapper.Map<OrderDto>(order);
            return orderDto;
        }

        public async Task<OrderDto> FindOrderFromFriendlyId(long friendlyId, Guid tenantId, bool includeIsEqualized)
        {
            var orderRepository = _orderRepository.CloneWithCustomClaimsProvider(new TemporaryClaimsProvider(tenantId, AppConstants.ADMINISTRATOR_ID, "Administrator", null, null));

            var order = await orderRepository.FindSingleFromFriendlyId(friendlyId, includeIsEqualized);
            if (order == null) return null;
            var orderDto = _mapper.Map<OrderDto>(order);
            return orderDto;
        }
    }
}
