using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Munk.AspNetCore;
using Munk.Utils.Object;
using Newtonsoft.Json;
using NxB.Allocating.Shared.Infrastructure;
using NxB.Allocating.Shared.Model;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;
using NxB.Dto.Clients;
using NxB.Dto.OrderingApi;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Infrastructure;

namespace NxB.BookingApi.Models
{
    public class OrderFactory
    {
        private readonly IClaimsProvider _claimsProvider;
        private readonly IFriendlyOrderIdProvider _friendlyOrderIdProvider;
        private readonly IAuthorTranslator<AppDbContext> _authorTranslator;
        private readonly IMapper _mapper;
        private readonly AppDbContext _appDbContext;
        private readonly IRentalSubTypeClient _rentalSubTypeClient;

        public OrderFactory(IClaimsProvider claimsProvider, IFriendlyOrderIdProvider friendlyOrderIdProvider, IAuthorTranslator<AppDbContext> authorTranslator, AppDbContext appDbContext, IMapper mapper, IRentalSubTypeClient rentalSubTypeClient)
        {
            _claimsProvider = claimsProvider;
            _friendlyOrderIdProvider = friendlyOrderIdProvider;
            _authorTranslator = authorTranslator;
            _appDbContext = appDbContext;
            _mapper = mapper;
            _rentalSubTypeClient = rentalSubTypeClient;
        }

        public OrderDto Map(Order model, IAllocationStateRepository allocationStateRepository)
        {
            if (model == null) return null;
            var dto = _mapper.Map<OrderDto>(model);
            AugmentSubOrders(dto.SubOrders, allocationStateRepository);
            return dto;
        }

        public OrderDto Map(Order model)
        {
            if (model == null) return null;
            var dto = _mapper.Map<OrderDto>(model);
            return dto;
        }

        private void AugmentSubOrders(List<SubOrderDto> subOrderDtos, IAllocationStateRepository allocationStateRepository)
        {
            var orderLineDtos = subOrderDtos.SelectMany(x => x.OrderLines).ToList();
            orderLineDtos.ForEach(x => x.CreateAuthorName = _authorTranslator.GetName(x.CreateAuthorId, _appDbContext));

            var subOrderSectionDtos = subOrderDtos.SelectMany(x => x.SubOrderSections).ToList();
            subOrderSectionDtos.ForEach(x => x.CreateAuthorName = _authorTranslator.GetName(x.CreateAuthorId, _appDbContext));

            if (allocationStateRepository != null)
            {
                foreach (var subOrderDto in subOrderDtos)
                {
                    var allocationState = allocationStateRepository.FindSingleOrDefault(subOrderDto.Id);
                    if (allocationState == null) //HACK. should never happen
                    {
                        subOrderDto.AllocationState =
                            _mapper.Map<AllocationStateDto>(new AllocationState(subOrderDto.Id, _claimsProvider.GetUserId(), _claimsProvider.GetTenantId()));
                    }
                    else
                    {
                        subOrderDto.AllocationState = _mapper.Map<AllocationStateDto>(allocationState);
                    }
                }
            }
        }

        public TimeBasedOrderLineDto MapTimeBasedOrderLine(TimedBasedOrderLine model)
        {
            if (model is GuestOrderLine)
            {
                return MapGuestOrderLine((GuestOrderLine)model);
            }
            if (model is AllocationOrderLine)
            {
                return MapAllocationOrderLine((AllocationOrderLine)model);
            }

            throw new ArgumentException("Could not map orderline of type " + model.GetType());
        }

        public OrderLineDto MapOrderLine(OrderLine model)
        {
            if (model is ArticleOrderLine)
            {
                return MapArticleOrderLine((ArticleOrderLine)model);
            }
            if (model is DiscountOrderLine)
            {
                return MapDiscountOrderLine((DiscountOrderLine)model);
            }
            if (model is SubOrderDiscountLine)
            {
                return MapSubOrderDiscountLine((SubOrderDiscountLine)model);
            }
            if (model is TimedBasedOrderLine)
            {
                return MapTimeBasedOrderLine((TimedBasedOrderLine)model);
            }
            throw new ArgumentException("Could not map orderline of type " + model.GetType());
        }

        private AllocationOrderLineDto MapAllocationOrderLine(AllocationOrderLine model)
        {
            var dto = _mapper.Map<AllocationOrderLineDto>(model);
            dto.CreateAuthorName = _authorTranslator.GetName(dto.CreateAuthorId, _appDbContext);
            return dto;
        }

        private GuestOrderLineDto MapGuestOrderLine(GuestOrderLine model)
        {
            var dto = _mapper.Map<GuestOrderLineDto>(model);
            dto.CreateAuthorName = _authorTranslator.GetName(dto.CreateAuthorId, _appDbContext);
            return dto;
        }

        private ArticleOrderLineDto MapArticleOrderLine(ArticleOrderLine model)
        {
            var dto = _mapper.Map<ArticleOrderLineDto>(model);
            dto.CreateAuthorName = _authorTranslator.GetName(dto.CreateAuthorId, _appDbContext);
            return dto;
        }

        private OrderLineDto MapDiscountOrderLine(DiscountOrderLine model)
        {
            var dto = _mapper.Map<DiscountOrderLineDto>(model);
            dto.CreateAuthorName = _authorTranslator.GetName(dto.CreateAuthorId, _appDbContext);
            return dto;
        }

        private OrderLineDto MapSubOrderDiscountLine(SubOrderDiscountLine model)
        {
            var dto = _mapper.Map<SubOrderDiscountLineDto>(model);
            dto.CreateAuthorName = _authorTranslator.GetName(dto.CreateAuthorId, _appDbContext);
            return dto;
        }

        public List<OrderLineDto> MapOrderLines(IEnumerable<OrderLine> model)
        {
            return model.Select(MapOrderLine).ToList();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Order Create(CreateOrderDto createOrderDto, IAllocationStateRepository allocationStateRepository, long? overrideFriendlyId = null)
        {
            if (createOrderDto == null) throw new ArgumentNullException(nameof(createOrderDto));
            if (createOrderDto.SubOrders == null) throw new ArgumentNullException(nameof(createOrderDto.SubOrders));
            if (createOrderDto.SubOrders.None()) throw new CreateOrderException($"New order/booking must contain at least 1 suborder, {JsonConvert.SerializeObject(createOrderDto)}.");
            if (!createOrderDto.SubOrders.TrueForAll(x => x.AllocationOrderLines != null && x.AllocationOrderLines.Count > 0)) throw new CreateOrderException($"New order/booking contains at least one suborder with no orderlines. All suborders must contain at least one orderline, {JsonConvert.SerializeObject(createOrderDto)}.");
            if (!createOrderDto.SubOrders.SelectMany(x => x.AllocationOrderLines).ToList().TrueForAll(x => x.ResourceId != Guid.Empty)) throw new CreateOrderException($"New order/booking contains at least one allocationorderline with no allocation. All allocationorderlines must have an allocation, {JsonConvert.SerializeObject(createOrderDto)}.");

            var order = _mapper.Map<CreateOrderDto, Order>(createOrderDto);

            InitializeOrder(overrideFriendlyId, order);

            order.AddOrAppendToSubOrders(order.SubOrders, this._claimsProvider.GetUserId());
            InitializeAllocationState(order, allocationStateRepository);
            return order;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Order Create(Guid orderId, CreateOrderDto createOrderDto, IAllocationStateRepository allocationStateRepository, long? overrideFriendlyId = null)
        {
            var order = Create(createOrderDto, allocationStateRepository, overrideFriendlyId);
            order.Id = orderId;
            return order;
        }

        /// <summary>
        /// Creates a new order, containing an already existing SubOrder, that has been moved from an existing Order
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="existingSubOrder"></param>
        /// <returns></returns>
        public Order Create(Guid accountId, SubOrder existingSubOrder)
        {
            if (accountId == Guid.Empty) throw new ArgumentNullException(nameof(accountId));
            if (existingSubOrder == null) throw new ArgumentNullException(nameof(existingSubOrder));
            if (existingSubOrder.Order != null) throw new ArgumentException(nameof(existingSubOrder.Order));
            if (existingSubOrder.OrderId != Guid.Empty) throw new ArgumentException(nameof(existingSubOrder.Order));

            var order = new Order(Guid.NewGuid());
            InitializeOrder(null, order);
            order.AccountId = accountId;
            order.AddExistingSubOrder(existingSubOrder);

            return order;
        }

        private void InitializeOrder(long? overrideFriendlyId, Order order)
        {
            var tenantId = _claimsProvider.GetTenantId();
            order.Id = Guid.NewGuid();
            order.FriendlyId = overrideFriendlyId ?? (int)_friendlyOrderIdProvider.GenerateNextFriendlyOrderId();
            order.TenantId = tenantId;
        }

        private void InitializeAllocationState(Order order, IAllocationStateRepository allocationStateRepository)
        {
            if (allocationStateRepository == null) return;
            foreach (var subOrder in order.SubOrders)
            {
                if (allocationStateRepository.FindSingleOrDefault(subOrder.Id) == null)
                {
                    var allocationState =
                        new AllocationState(subOrder.Id, _claimsProvider.GetUserId(), _claimsProvider.GetTenantId());
                    allocationStateRepository.Add(allocationState);
                }
            }
        }

        public async Task<(List<SubOrder> created, List<SubOrder> modified)> MergeOrAddSubOrders(AddToOrderDto addToOrderDto, Order existingOrder, IAllocationStateRepository allocationStateRepository, IOrderRepository orderRepository)
        {
            if (addToOrderDto.Id != existingOrder.Id) throw new AddSubOrdersException($"Existing orderId {existingOrder.Id} does not match {addToOrderDto.Id}");
            if (addToOrderDto == null) throw new ArgumentNullException(nameof(addToOrderDto));
            if (addToOrderDto.SubOrders == null) throw new ArgumentNullException(nameof(addToOrderDto.SubOrders));
            if (addToOrderDto.SubOrders.None()) throw new CreateOrderException($"New order/booking must contain at least 1 suborder, {JsonConvert.SerializeObject(addToOrderDto)}.");
            if (!addToOrderDto.SubOrders.TrueForAll(x => (x.AllocationOrderLines != null && x.AllocationOrderLines.Count > 0) || x.Id != Guid.Empty)) throw new CreateOrderException($"AddTo order/booking contains at least one suborder with no orderlines. All suborders must contain at least one orderline, {JsonConvert.SerializeObject(addToOrderDto)}.");
            if (!addToOrderDto.SubOrders.SelectMany(x => x.AllocationOrderLines).ToList().TrueForAll(x => x.ResourceId != Guid.Empty)) throw new CreateOrderException($"AddTo order/booking contains at least one allocationorderline with no allocation. All allocationorderlines must have an allocation, {JsonConvert.SerializeObject(addToOrderDto)}.");

            //var allocationOrderLineDtos = addToOrderDto.SubOrders.SelectMany(x => x.AllocationOrderLines).ToList();
            //await PreProcessOrderLineDtos(allocationOrderLineDtos);
            var subOrders = _mapper.Map<List<CreateOrAddToSubOrderDto>, List<SubOrder>>(addToOrderDto.SubOrders);
            var createdOrAddedSubOrders = existingOrder.AddOrAppendToSubOrders(subOrders, this._claimsProvider.GetUserId());
            ModifyFromExistingSubOrders(existingOrder, addToOrderDto, orderRepository);
            InitializeAllocationState(existingOrder, allocationStateRepository);
            return createdOrAddedSubOrders;
        }

        private void ModifyFromExistingSubOrders(Order existingOrder, AddToOrderDto addToOrderDto, IOrderRepository orderRepository)
        {
            var subOrderDiscountIds = addToOrderDto.SubOrders.SelectMany(x => x.SubOrderDiscountIdsMarkedForDeletion).ToList();
            existingOrder.MarkSubOrderDiscountsAsDeleted(existingOrder.SubOrders, subOrderDiscountIds);
        }

        //private async Task PreProcessOrderLineDtos(List<CreateAllocationOrderLineDto> orderLines)
        //{
        //    var allocationOrderLinesWithRentalSubTypeId = orderLines.Where(x => x.RentalSubTypeId != null).ToList();

        //    foreach (var allocationOrderLineDto in allocationOrderLinesWithRentalSubTypeId)
        //    {
        //        var rentalSubTypeDto = await _rentalSubTypeClient.FindSingleOrDefault(allocationOrderLineDto.RentalSubTypeId.Value);
        //        allocationOrderLineDto.Text += rentalSubTypeDto.NameTranslations
        //    }
        //}
    }
}
