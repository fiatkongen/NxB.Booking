using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using Munk.Utils.Object;
using Newtonsoft.Json.Linq;
using NxB.BookingApi.Models;
using NxB.BookingApi.Models.Exceptions;
using NxB.Domain.Common.Constants;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;
using NxB.Dto.Clients;
using NxB.Dto.DocumentApi;
using NxB.Dto.OrderingApi;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Infrastructure;

namespace NxB.BookingApi.Controllers.Ordering
{
    [Produces("application/json")]
    [Route("order")]
    [Authorize]
    [ApiValidationFilter]
    public class OrderingController : BaseController
    {
        private readonly IAllocationRepository _allocationRepository;
        private readonly OrderFactory _orderFactory;
        private readonly IOrderRepository _orderRepository;
        private readonly AppDbContext _appDbContext;
        private readonly IRentalCaches _rentalCaches;
        private readonly IClaimsProvider _claimsProvider;
        private readonly IRentalUnitRepository _rentalUnitRepository;
        private readonly IAllocationStateRepository _allocationStateRepository;
        private readonly IVoucherClient _voucherClient;
        private readonly IOrderValidator _orderValidator;
        private readonly ICounterPushUpdateService _counterPushUpdateService;
        private readonly IGroupedBroadcasterClient _groupedBroadcasterClient;
        private readonly TelemetryClient _telemetryClient;
        private readonly ITaxHelper _taxHelper;
        private readonly IMemCacheActor _memCacheActor;
        private readonly IMapper _mapper;

        public OrderingController(
            OrderFactory orderFactory,
            IOrderRepository orderRepository,
            AppDbContext appDbContext,
            IRentalCaches rentalCaches,
            IClaimsProvider claimsProvider,
            IRentalUnitRepository rentalUnitRepository,
            IAllocationStateRepository allocationStateRepository,
            IVoucherClient voucherClient,
            IAllocationRepository allocationRepository,
            IOrderValidator orderValidator,
            ICounterPushUpdateService counterPushUpdateService,
            IGroupedBroadcasterClient groupedBroadcasterClient,
            TelemetryClient telemetryClient,
            ITaxHelper taxHelper, 
            IMemCacheActor memCacheActor, IMapper mapper)
        {
            _orderFactory = orderFactory;
            _orderRepository = orderRepository;
            _appDbContext = appDbContext;
            _rentalCaches = rentalCaches;
            _claimsProvider = claimsProvider;
            _rentalUnitRepository = rentalUnitRepository;
            _allocationStateRepository = allocationStateRepository;
            _voucherClient = voucherClient;
            _allocationRepository = allocationRepository;
            _orderValidator = orderValidator;
            _counterPushUpdateService = counterPushUpdateService;
            _groupedBroadcasterClient = groupedBroadcasterClient;
            _telemetryClient = telemetryClient;
            _taxHelper = taxHelper;
            _memCacheActor = memCacheActor;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("")]
        public async Task<ObjectResult> FindOrder([Required(AllowEmptyStrings = false)] string orderId, bool includeIsEqualized = false)
        {
            bool isFriendly = int.TryParse(orderId, out var friendlyId);
            Order order;

            if (isFriendly)
            {
                order = await _orderRepository.FindSingleFromFriendlyId(friendlyId, includeIsEqualized);
            }
            else
            {
                order = await _orderRepository.FindSingle(Guid.Parse(orderId), includeIsEqualized);
            }

            var orderDto = _orderFactory.Map(order, this._allocationStateRepository);
            return new OkObjectResult(orderDto);
        }

        [HttpGet]
        [Route("orderlineid")]
        public async Task<ObjectResult> FindOrderFromOrderLineId(Guid orderLineId, bool includeIsEqualized = false)
        {
            Order order;
            order = await _orderRepository.FindSingleOrDefaultOrderIdFromOrderLineId(orderLineId);
            var orderDto = _orderFactory.Map(order, this._allocationStateRepository);
            return new OkObjectResult(orderDto);
        }


        [HttpGet]
        [AllowAnonymous]
        [Route("exists")]
        public async Task<bool> ExistsOrder(Guid orderId)
        {
            return await _orderRepository.Exists(orderId);
        }

        [HttpGet]
        [Route("suborderid")]
        public async Task<ObjectResult> FindFromSubOrder(Guid subOrderId)
        {
            var order = await _orderRepository.FindSingleOrDefaultFromSubOrderId(subOrderId, false);
            OrderDto orderDto = null;
            if (order != null)
            {
                orderDto = _orderFactory.Map(order, this._allocationStateRepository);
            }
            return new OkObjectResult(orderDto);
        }

        [HttpGet]
        [Route("query/allocation")]
        public async Task<ObjectResult> FindOrderIdFromAllocationId(Guid allocationId)
        {
            var order = await _orderRepository.FindSingleOrDefaultOrderIdFromAllocationId(allocationId);
            var orderDto = _orderFactory.Map(order, this._allocationStateRepository);
            return new OkObjectResult(orderDto);
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAllOrders(DateTime start, DateTime end)
        {
            var dateInterval = new DateInterval(start, end);
            var orders = (await _orderRepository.FindAll(dateInterval)).ToList();
            var orderDtos = orders.Select(x => _orderFactory.Map(x, null));
            return new OkObjectResult(orderDtos);
        }

        [HttpPost]
        [Route("")]
        public async Task<ObjectResult> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
             using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }, TransactionScopeAsyncFlowOption.Enabled);

            var order = _orderFactory.Create(createOrderDto, _allocationStateRepository, createOrderDto.OverrideFriendlyId);
            _orderRepository.Add(order);

            try
            {
                var addedOrderLines = _appDbContext.ChangeTracker.Entries().Where(e => e.State == EntityState.Added)
                    .Select(x => x.Entity).OfType<ResourceBasedOrderLine>().Cast<ITaxableItem>().ToList();
                await _taxHelper.AddTaxToOrder(addedOrderLines, _appDbContext);
                await _orderValidator.ValidateOrderAndInitializeCaches(order);
            }
            catch (AvailabilityException availabilityException)
            {
                return new BadRequestObjectResult(availabilityException.Message);
            }

            var orderDto = _orderFactory.Map(order, this._allocationStateRepository);
            var createdResult = new CreatedResult(new Uri("?id=" + orderDto.Id, UriKind.Relative), orderDto);

            await _appDbContext.SaveChangesAsync();
            transactionScope.Complete();

           // _digitalGuestClientHelper.TryCheckDigitalGuestCreateOrModifyFireAndForget(orderDto, orderDto.SubOrders, DigitalGuestAction.Create, this.GetTenantId());
            await _counterPushUpdateService.TryPushUpdateMissingArrivalsCounter().CatchExceptionAndLogToTelemetry(_telemetryClient);
            _groupedBroadcasterClient.TryOrderModified(order.Id).FireAndForgetLogToTelemetry(_telemetryClient); ;
            await _memCacheActor.PublishOrderCreated(GetTenantId(), orderDto).CatchExceptionAndLogToTelemetry(_telemetryClient);

            return createdResult;
        }


        [HttpPost]
        [Route("add")]
        public async Task<ObjectResult> AddToOrder([FromBody] AddToOrderDto addToOrderDto)
        {
            Order order;
            (List<SubOrder> created, List<SubOrder> modified) createdOrModified;

            OrderDto originalOrderDto = null;

            using (var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }, TransactionScopeAsyncFlowOption.Enabled))
            {
                order = await _orderRepository.FindSingle(addToOrderDto.Id, true);
                originalOrderDto = _orderFactory.Map(order, this._allocationStateRepository);

                try
                {
                    createdOrModified = await _orderFactory.MergeOrAddSubOrders(addToOrderDto, order, _allocationStateRepository, _orderRepository);
                    var addedOrderLines = _appDbContext.ChangeTracker.Entries().Where(e => e.State == EntityState.Added)
                        .Select(x => x.Entity).OfType<ResourceBasedOrderLine>().Cast<ITaxableItem>().ToList();
                    await _taxHelper.AddTaxToOrder(addedOrderLines, _appDbContext);
                    await _orderValidator.ValidateOrderAndInitializeCaches(order);
                }
                catch (AvailabilityOverReleasedException exception)
                {
                    return new BadRequestObjectResult(exception.Message);
                }

                catch (AvailabilityException availabilityException)
                {
                    return new BadRequestObjectResult(availabilityException.Message);
                }

                await _appDbContext.SaveChangesAsync();
                transactionScope.Complete();
            }

            var orderDto = _orderFactory.Map(order, this._allocationStateRepository);

            var createdSubOrderDtos = orderDto.SubOrders.Where(x => createdOrModified.created.Any(su => su.Id == x.Id)).ToList();
            //_digitalGuestClientHelper.TryCheckDigitalGuestCreateOrModifyFireAndForget(orderDto, createdSubOrderDtos, DigitalGuestAction.Create, this.GetTenantId());

            var modifiedSubOrderDtos = orderDto.SubOrders.Where(x => createdOrModified.modified.Any(su => su.Id == x.Id)).ToList();
           // _digitalGuestClientHelper.TryCheckDigitalGuestCreateOrModifyFireAndForget(orderDto, modifiedSubOrderDtos, DigitalGuestAction.Modify, this.GetTenantId());

            await _counterPushUpdateService.TryPushUpdateMissingArrivalsCounter().CatchExceptionAndLogToTelemetry(_telemetryClient);
            _groupedBroadcasterClient.TryOrderModified(order.Id).FireAndForgetLogToTelemetry(_telemetryClient); ;

            await _memCacheActor.TryPublishOrderModified(GetTenantId(), originalOrderDto, modifiedSubOrderDtos, createdSubOrderDtos, _telemetryClient);

            return new OkObjectResult(orderDto);
        }


        [HttpPost]
        [Route("note")]
        public async Task<IActionResult> ModifyOrderNote([FromBody] ModifyOrderNoteDto modifyOrderNoteDto)
        {
            this._orderRepository.UpdateOrderNote(modifyOrderNoteDto.OrderId, modifyOrderNoteDto.Note, modifyOrderNoteDto.NoteState);
            await this._appDbContext.SaveChangesAsync();

            _groupedBroadcasterClient.TryOrderModified(modifyOrderNoteDto.OrderId).FireAndForgetLogToTelemetry(_telemetryClient); ;
            return Ok();
        }

        [HttpPost]
        [Route("note/append")]
        public async Task<IActionResult> AppendToOrderNote([FromBody] ModifyOrderNoteDto modifyOrderNoteDto)
        {
            this._orderRepository.AppendToOrderNote(modifyOrderNoteDto.OrderId, modifyOrderNoteDto.Note);
            await this._appDbContext.SaveChangesAsync();

            _groupedBroadcasterClient.TryOrderModified(modifyOrderNoteDto.OrderId).FireAndForgetLogToTelemetry(_telemetryClient); ;
            return Ok();
        }

        [HttpPost]
        [Route("suborder/note")]
        public IActionResult ModifySubOrderNote([FromBody] ModifySubOrderNoteDto modifySubOrderNoteDto)
        {
            this._orderRepository.UpdateSubOrderNote(modifySubOrderNoteDto.SubOrderId, modifySubOrderNoteDto.Note, modifySubOrderNoteDto.NoteState);
            this._appDbContext.SaveChanges();
            return Ok();
        }

        [HttpGet]
        [Route("timebasedorderlines/dategaps")]
        public async Task<ObjectResult> CalculateDateGapsForSubOrder([NoEmpty] Guid subOrderId, DateTime newStart, DateTime newEnd)
        {
            var newDateInterval = new DateInterval(newStart, newEnd);
            var order = await _orderRepository.FindSingleFromSubOrderId(subOrderId, false);
            var subOrder = order.SubOrders.Single(x => x.Id == subOrderId);
            var timeBasedOrderLineGaps = await subOrder.CalculateDateGapsForTimeBasedOrderLines(newDateInterval, this._claimsProvider.GetUserId());
            var timeBasedOrderLineGapsDtos = timeBasedOrderLineGaps.Select(x => _orderFactory.MapTimeBasedOrderLine(x)).ToList();

            var timeBasedOrderLinesDto = new TimeBasedOrderLinesDto
            {
                AllocationOrderLines = timeBasedOrderLineGapsDtos.OfType<AllocationOrderLineDto>().ToList(),
                GuestOrderLines = timeBasedOrderLineGapsDtos.OfType<GuestOrderLineDto>().ToList()
            };

            return new OkObjectResult(timeBasedOrderLinesDto);
        }

        [HttpPost]
        [Route("suborder/revert")]
        public async Task<ObjectResult> RevertSubOrder([NoEmpty] Guid subOrderId, bool revertingForCancellation = true)
        {
            var createAuthorId = _claimsProvider.GetUserId();

            Order order;
            using (var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }, TransactionScopeAsyncFlowOption.Enabled))
            {
                order = await _orderRepository.FindSingleFromSubOrderId(subOrderId, true);

                var subOrder = order.SubOrders.Single(x => x.Id == subOrderId);
                if (!subOrder.IsEqualized)
                {
                    var orderLineIds = subOrder.OrderLines.Select(x => x.Id).ToList();
                    await CheckOrderLinesAreNotInvoiced(order, orderLineIds, subOrder);

                    order.RevertSubOrder(subOrderId, createAuthorId);
                    try
                    {
                        await _orderValidator.ValidateOrderAndInitializeCaches(order);
                    }
                    catch (AvailabilityException availabilityException)
                    {
                        return new BadRequestObjectResult(availabilityException.Message);
                    }

                    await _appDbContext.SaveChangesAsync();
                    transactionScope.Complete();
                }
            }

            var orderDto = _orderFactory.Map(order, this._allocationStateRepository);
            _groupedBroadcasterClient.TryOrderModified(order.Id).FireAndForgetLogToTelemetry(_telemetryClient); ;


            if (revertingForCancellation)
            {
                await _memCacheActor.PublishSubOrderCancelled(GetTenantId(), subOrderId);
            }

            return new OkObjectResult(orderDto);
        }

        private async Task CheckOrderLinesAreNotInvoiced(Order order, List<Guid> orderLineIds, SubOrder subOrder)
        {
            var invoicedOrderLineIds = (await _voucherClient.GetInvoicedOrderLineIds(order.Id)).Select(x => x.Key).ToList();

            if (invoicedOrderLineIds.Intersect(orderLineIds).Any())
            {
                throw new RevertSubOrderException(subOrder,
                    "Kan ikke tilbageføre linier som er faktureret. Kreditér de fakturerede linier og forsøg igen.");
            }
        }

        [HttpPost]
        [Route("suborder/release")]
        public async Task<ObjectResult> ReleaseSubOrder([NoEmpty] Guid subOrderId)
        {
            var createAuthorId = _claimsProvider.GetUserId();

            Order order;
            OrderDto originalOrderDto;

            using (var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }, TransactionScopeAsyncFlowOption.Enabled))
            {
                order = await _orderRepository.FindSingleFromSubOrderId(subOrderId, true);
                originalOrderDto = _orderFactory.Map(order);

                var subOrder = order.SubOrders.Single(x => x.Id == subOrderId);
                if (!subOrder.IsEqualized)
                {
                    try
                    {
                        order.ReleaseSubOrder(subOrderId, createAuthorId);
                        await _orderValidator.ValidateOrderAndInitializeCaches(order);
                    }
                    catch (AvailabilityOverReleasedException exception)
                    {
                        return new BadRequestObjectResult(exception.Message);
                    }
                    await _appDbContext.SaveChangesAsync();
                    transactionScope.Complete();
                }
            }

            var orderDto = _orderFactory.Map(order, this._allocationStateRepository);
            var modifiedOrderDto = orderDto.SubOrders.Single(x => x.Id == subOrderId);
            await _memCacheActor.TryPublishOrderModified(GetTenantId(), originalOrderDto, new List<SubOrderDto> { modifiedOrderDto }, new List<SubOrderDto>(), _telemetryClient);

            return new OkObjectResult(orderDto);
        }

        [HttpPost]
        [Route("suborder/move/customer")]
        public async Task<ObjectResult> MoveSubOrderToAccount([NoEmpty] Guid subOrderId, [NoEmpty] Guid accountId)
        {
            using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }, TransactionScopeAsyncFlowOption.Enabled);
            var order = await _orderRepository.FindSingleFromSubOrderId(subOrderId, true);

            var subOrder = order.RemoveSubOrder(subOrderId);
            if (subOrder.IsEqualized) throw new MoveSubOrderException(subOrderId, order, null);
            var newOrder = _orderFactory.Create(accountId, subOrder);

            try
            {
                await _orderValidator.ValidateOrderAndInitializeCaches(order);
                await _orderValidator.ValidateOrderAndInitializeCaches(newOrder);
            }
            catch (AvailabilityException availabilityException)
            {
                return new BadRequestObjectResult(availabilityException.Message);
            }

            await _appDbContext.SaveChangesAsync();
            var orderDto = _orderFactory.Map(newOrder, this._allocationStateRepository);
            transactionScope.Complete();

            _groupedBroadcasterClient.TryOrderModified(order.Id).FireAndForgetLogToTelemetry(_telemetryClient); ;
            return new OkObjectResult(orderDto);
        }

        [HttpPost]
        [Route("suborder/move/booking")]
        public async Task<ObjectResult> MoveSubOrderToBooking([NoEmpty] Guid subOrderId, long bookingId)
        {
            using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }, TransactionScopeAsyncFlowOption.Enabled);
            var sourceOrder = await this._orderRepository.FindSingleFromSubOrderId(subOrderId, true);
            var subOrder = sourceOrder.RemoveSubOrder(subOrderId);
            if (subOrder.IsEqualized) throw new MoveSubOrderException(subOrderId, sourceOrder, null);

            var destinationOrder = await this._orderRepository.FindSingleFromFriendlyId(bookingId, true);
            destinationOrder.AddExistingSubOrder(subOrder);

            try
            {
                await _orderValidator.ValidateOrderAndInitializeCaches(sourceOrder);
                await _orderValidator.ValidateOrderAndInitializeCaches(destinationOrder);
            }
            catch (AvailabilityException availabilityException)
            {
                return new BadRequestObjectResult(availabilityException.Message);
            }


            await _appDbContext.SaveChangesAsync();
            var orderDto = _orderFactory.Map(destinationOrder, this._allocationStateRepository);
            transactionScope.Complete();

            _groupedBroadcasterClient.TryOrderModified(orderDto.Id).FireAndForgetLogToTelemetry(_telemetryClient); ;

            return new OkObjectResult(orderDto);
        }

        [HttpGet]
        [Route("suborder/orderlines/unreverted")]
        public async Task<ObjectResult> BuildUnRevertedOrderLines([NoEmpty] Guid subOrderId)
        {
            var createAuthorId = _claimsProvider.GetUserId();
            var order = await this._orderRepository.FindSingleFromSubOrderId(subOrderId, true);
            var subOrder = order.SubOrders.Single(x => x.Id == subOrderId);

            var unRevertedOrderLines = subOrder.BuildUnRevertedOrderLines(createAuthorId);
            var orderLineDtos = _orderFactory.MapOrderLines(unRevertedOrderLines);

            var orderLinesDto = new OrderLinesDto
            {
                AllocationOrderLines = orderLineDtos.OfType<AllocationOrderLineDto>().ToList(),
                GuestOrderLines = orderLineDtos.OfType<GuestOrderLineDto>().ToList(),
                ArticleOrderLines = orderLineDtos.OfType<ArticleOrderLineDto>().ToList(),
                DiscountOrderLines = orderLineDtos.OfSpecificType<DiscountOrderLineDto>(nameof(DiscountOrderLineDto)).ToList(),
                SubOrderDiscountLines = orderLineDtos.OfType<SubOrderDiscountLineDto>().ToList(),
            };

            return new OkObjectResult(orderLinesDto);
        }

        [HttpPost]
        [Route("allocationorderlines/swap")]
        public async Task<ObjectResult> SwapAllocationOrderLines([FromBody] SwapAllocationsDto swapAllocationsDto)
        {
            var createAuthorId = _claimsProvider.GetUserId();
            var tenantId = _claimsProvider.GetTenantId();

            var order1 = await this._orderRepository.FindSingleFromSubOrderId(swapAllocationsDto.SubOrderId1, false);
            var originalOrderDto = _orderFactory.Map(order1);
            var subOrder1 = order1.SubOrders.Single(x => x.Id == swapAllocationsDto.SubOrderId1);
            var subOrder1AllocationOrderLines = subOrder1.AllocationOrderLines.Where(x => !x.IsEqualized && x.ResourceId == swapAllocationsDto.RentalUnitId1).ToList();
            var resourceText1 = subOrder1AllocationOrderLines[0].Text;

            var order2 = await this._orderRepository.FindSingleFromSubOrderId(swapAllocationsDto.SubOrderId2, false);
            var subOrder2 = order2.SubOrders.Single(x => x.Id == swapAllocationsDto.SubOrderId2);
            var subOrder2AllocationOrderLines = subOrder2.AllocationOrderLines.Where(x => !x.IsEqualized && x.ResourceId == swapAllocationsDto.RentalUnitId2).ToList();
            var resourceText2 = subOrder2AllocationOrderLines[0].Text;

            if (order1.SubOrders.Any(x => x.ArticleOrderLines.Any(x => x.MeterReference.HasValue && !x.IsEqualized)))
            {
                throw new SwapAllocationsException(swapAllocationsDto, resourceText1, resourceText2, "Kan ikke ombytte da " + resourceText1 + " indeholder en måleraflæsning.");
            }
            if (order2.SubOrders.Any(x => x.ArticleOrderLines.Any(x => x.MeterReference.HasValue && !x.IsEqualized)))
            {
                throw new SwapAllocationsException(swapAllocationsDto, resourceText1, resourceText2, "Kan ikke ombytte da " + resourceText2 + " indeholder en måleraflæsning.");
            }

            if (swapAllocationsDto.SubOrderId1 == swapAllocationsDto.SubOrderId2) throw new SwapAllocationsException(swapAllocationsDto, resourceText1, resourceText2, "Kan ikke ombytte indenfor samme booking");
            if (swapAllocationsDto.RentalUnitId1 == swapAllocationsDto.RentalUnitId2) throw new SwapAllocationsException(swapAllocationsDto, resourceText1, resourceText2, "Kan ikke ombytte til samme enhed");

            var rentalUnit1 = _rentalUnitRepository.FindSingle(swapAllocationsDto.RentalUnitId1);
            var rentalUnit2 = _rentalUnitRepository.FindSingle(swapAllocationsDto.RentalUnitId2);

            if (rentalUnit1.RentalCategoryId != rentalUnit2.RentalCategoryId) throw new SwapAllocationsException(swapAllocationsDto, resourceText1, resourceText2, "Enheder skal være af samme type");

            await CheckOrderLinesAreNotInvoiced(order1, subOrder1AllocationOrderLines.Select(x => x.Id).ToList(), subOrder1);
            await CheckOrderLinesAreNotInvoiced(order2, subOrder2.AllocationOrderLines.Select(x => x.Id).ToList(), subOrder2);

            using (var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }, TransactionScopeAsyncFlowOption.Enabled))
            {
                foreach (var allocationOrderLine in subOrder1AllocationOrderLines)
                {
                    subOrder1.RevertOrderLine(createAuthorId, allocationOrderLine.Id);
                }

                foreach (var allocationOrderLine in subOrder2AllocationOrderLines)
                {
                    subOrder2.RevertOrderLine(createAuthorId, allocationOrderLine.Id);
                }

                foreach (var allocationOrderLine in subOrder1AllocationOrderLines)
                {
                    var clonedAllocationOrderLine = (AllocationOrderLine)allocationOrderLine.Clone();
                    clonedAllocationOrderLine.ResourceId = swapAllocationsDto.RentalUnitId2;
                    clonedAllocationOrderLine.Text = resourceText2;
                    //Hack
                    clonedAllocationOrderLine.Allocation.RentalUnitId = swapAllocationsDto.RentalUnitId2;
                    clonedAllocationOrderLine.Allocation.RentalUnitName = resourceText2;
                    subOrder1.OrderLines.Add(clonedAllocationOrderLine);
                }
                subOrder1.AugmentSubOrderSectionsAndOrderLines(createAuthorId, tenantId);
                subOrder1.EqualizeOrderLines();

                foreach (var allocationOrderLine in subOrder2AllocationOrderLines)
                {
                    var clonedAllocationOrderLine = (AllocationOrderLine)allocationOrderLine.Clone();
                    clonedAllocationOrderLine.ResourceId = swapAllocationsDto.RentalUnitId1;
                    clonedAllocationOrderLine.Text = resourceText1;
                    //Hack
                    clonedAllocationOrderLine.Allocation.RentalUnitId = swapAllocationsDto.RentalUnitId1;
                    clonedAllocationOrderLine.Allocation.RentalUnitName = resourceText1;
                    subOrder2.OrderLines.Add(clonedAllocationOrderLine);
                }
                subOrder2.AugmentSubOrderSectionsAndOrderLines(createAuthorId, tenantId);
                subOrder2.EqualizeOrderLines();

                try
                {
                    await _orderValidator.ValidateOrderAndInitializeCaches(new List<Order> { order1, order2 });
                }
                catch (AvailabilityException availabilityException)
                {
                    return new BadRequestObjectResult(availabilityException.Message);
                }

                await _appDbContext.SaveChangesAsync();

                transactionScope.Complete();
            }
            var orderDto1 = _orderFactory.Map(order1, this._allocationStateRepository);
            var orderDto2 = _orderFactory.Map(order2, this._allocationStateRepository);

            if (!Debugger.IsAttached)
            {
                _groupedBroadcasterClient.TryOrderModified(orderDto1.Id).FireAndForgetLogToTelemetry(_telemetryClient); ;
                _groupedBroadcasterClient.TryOrderModified(orderDto2.Id).FireAndForgetLogToTelemetry(_telemetryClient); ;
            }

            var modifiedSubOrderDto = orderDto1.SubOrders.Single(x => x.Id == swapAllocationsDto.SubOrderId1);
            await _memCacheActor.TryPublishOrderModified(GetTenantId(), originalOrderDto, new List<SubOrderDto> { modifiedSubOrderDto }, new List<SubOrderDto>(), _telemetryClient);

            return new OkObjectResult(new List<OrderDto> { orderDto1, orderDto2 });
        }

        [HttpGet]
        [Route("calculate/total")]
        public async Task<ObjectResult> CalculateOrderTotal([NoEmpty] Guid id)
        {
            var orderTotal = await _orderRepository.CalculateOrderTotal(id);
            dynamic response = new JObject();
            response.result = orderTotal;
            return new OkObjectResult(response);
        }

        [HttpGet]
        [Route("account/calculate/total")]
        public async Task<ObjectResult> CalculateAccountTotal([NoEmpty] Guid accountId)
        {
            var accountTotal = await _orderRepository.CalculateAccountTotal(accountId);
            dynamic response = new JObject();
            response.result = accountTotal;
            return new OkObjectResult(response);
        }

        [HttpGet]
        [Route("orderline/meter/lastreading")]
        public async Task<ObjectResult> GetLastMeterReading([NoEmpty] Guid subOrderId, [NoEmpty] Guid rentalUnitId)
        {
            var lastMeterReading = await _orderRepository.GetLastMeterReading(subOrderId, rentalUnitId);
            return new ObjectResult(lastMeterReading);
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        [Route("orderline")]
        public async Task<IActionResult> PermanentlyDeleteOrderLine(Guid id)
        {
            using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }, TransactionScopeAsyncFlowOption.Enabled);
            var deletedOrderLine = _orderRepository.DeleteOrderLine(id);

            Order order = null;
            OrderDto originalOrderDto = null;

            if (deletedOrderLine is AllocationOrderLine allocationOrderLine)
            {
                order = await _orderRepository.FindSingleOrDefaultOrderIdFromAllocationId(allocationOrderLine.AllocationId);
                originalOrderDto = _orderFactory.Map(order);
                var allocation = await _allocationRepository.FindSingle(allocationOrderLine.AllocationId);
                _allocationRepository.DeleteAllocation(allocationOrderLine.AllocationId);
                var orderDateInterval = order.DateInterval;
                await _rentalCaches.Initialize(orderDateInterval.Start, orderDateInterval.End);
                allocation.Number = 0 - allocation.Number;
                await _rentalCaches.AddAllocationsAndSave(new List<Allocation>
                {
                    allocation
                });

                var orderDto = _orderFactory.Map(order);
                var modifiedSubOrder = orderDto.SubOrders.Single(x => x.Id == deletedOrderLine.SubOrderId);
                await _memCacheActor.TryPublishOrderModified(GetTenantId(), originalOrderDto, new List<SubOrderDto> { modifiedSubOrder }, new List<SubOrderDto>(), _telemetryClient);
            }

            await _appDbContext.SaveChangesAsync();
            transactionScope.Complete();

            if (order != null)
            {
                _groupedBroadcasterClient.TryOrderModified(order.Id).FireAndForgetLogToTelemetry(_telemetryClient); ;
            }

            return Ok();
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        [Route("orderline/isequalized/remove")]
        public async Task<IActionResult> ToggleIsEqualizedOrderLine(Guid subOrderId, Guid orderLineId)
        {
            using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }, TransactionScopeAsyncFlowOption.Enabled);
            var order = await _orderRepository.FindSingleFromSubOrderId(subOrderId, false);
            var originalOrderDto = _orderFactory.Map(order);
            var subOrder = order.SubOrders.Single(x => x.Id == subOrderId);
            var orderLine = subOrder.OrderLines.Single(x => x.Id == orderLineId);
            orderLine.RemoveEqualize();

            await _appDbContext.SaveChangesAsync();
            transactionScope.Complete();

            _groupedBroadcasterClient.TryOrderModified(order.Id).FireAndForgetLogToTelemetry(_telemetryClient); ;

            var orderDto = _orderFactory.Map(order);
            var modifiedSubOrder = orderDto.SubOrders.Single(x => x.Id == subOrderId);
            await _memCacheActor.TryPublishOrderModified(GetTenantId(), originalOrderDto, new List<SubOrderDto> { modifiedSubOrder }, new List<SubOrderDto>(), _telemetryClient);
            return Ok();
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        [Route("orderline/all/update/tax")]
        public async Task<IActionResult> UpdateTaxesForAllOrderLines()
        {
            var orders = await _orderRepository.FindAll(new DateInterval(new DateTime(2000, 1, 1), new DateTime(2030, 1, 1)));
            _telemetryClient.TrackTrace("Updating taxes for " + orders.Count + " orders");
            int i = 0;
            foreach (var order in orders)
            {
                var orderLines = order.SubOrders.SelectMany(x => x.OrderLines).OfType<ResourceBasedOrderLine>().Cast<ITaxableItem>().ToList();

                await _taxHelper.UpdateTaxForOrder(orderLines, _appDbContext);
                await _appDbContext.SaveChangesAsync();
                _telemetryClient.TrackTrace("Updated tax for " + i++);
            }
            return Ok();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("tenantidfromsuborderid")]
        public async Task<Guid?> FindTenantIdFromSubOrderId(Guid subOrderId)
        {
            var tenantId = await _orderRepository.FindTenantIdFromSubOrderId(subOrderId);
            return tenantId;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("public/today")]
        public async Task<ObjectResult> FindPublicOrderIdFromOrderId(Guid orderId, Guid tenantId)
        {
            var orderRepository = _orderRepository.CloneWithCustomClaimsProvider(TemporaryClaimsProvider.CreateOnline(tenantId));
            var order = await orderRepository.FindSingle(orderId, false);
            var publicOrderDto = _mapper.Map<PublicOrderDto>(order);
            return new ObjectResult(publicOrderDto);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("tenantidfromorderid")]
        public async Task<Guid?> FindTenantIdFromOrderId(Guid orderId)
        {
            var tenantId = await _orderRepository.FindTenantIdFromOrderId(orderId);
            return tenantId;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("tenantidfromexternalorderid")]
        public async Task<Guid?> FindTenantIdFromExternalOrderId(string externalOrderId)
        {
            var tenantId = await _orderRepository.FindTenantIdFromExternalOrderId(externalOrderId);
            return tenantId;
        }

        [HttpGet]
        [Route("orderfromexternalorderid")]
        public async Task<ObjectResult> FindOrderFromExternalOrderId(string externalOrderId)
        {
            var order = await _orderRepository.FindSingleFromExternalOrderId(externalOrderId);
            
            var orderDto = _orderFactory.Map(order, null);
            return new OkObjectResult(orderDto);
        }

        [HttpPut]
        [Route("onlinetransactiondetails")]
        public async Task<IActionResult> ModifyOrderOnlineTransactionDetails([FromBody] ModifyOrderOnlineTransactionDetails modifyOrderOnlineTransactionDetails)
        {
            var order = await _orderRepository.FindSingleOrDefault(modifyOrderOnlineTransactionDetails.OrderId, false);
            order.OnlineTransactionDetails = modifyOrderOnlineTransactionDetails.TransactionDetails;
            await this._appDbContext.SaveChangesAsync();
            return Ok();
        }
    }
}

