using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NxB.BookingApi.Models;
using NxB.BookingApi.Models.Exceptions;
using NxB.Domain.Common.Interfaces;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Models;
using NxB.Remoting.Interfaces.PricingApi;
using NxB.BookingApi.Extensions;

namespace NxB.BookingApi.Infrastructure
{
    public class OrderValidator : IOrderValidator
    {
        private readonly AppDbContext _appDbContext;
        private readonly IRentalCaches _rentalCaches;
        private readonly AvailabilityGuard _availabilityGuard;
        private readonly IInvoicedOrderLinesValidator _invoicedOrderLinesValidator;
        private readonly IPriceProfilesValidator _priceProfilesValidator;
        private readonly IClaimsProvider _claimsProvider;
        private readonly IRentalUnitRepository _rentalUnitRepository;

        public OrderValidator(AppDbContext appDbContext, IRentalCaches rentalCaches, AvailabilityGuard availabilityGuard, IInvoicedOrderLinesValidator invoicedOrderLinesValidator, IPriceProfilesValidator priceProfilesValidator, IClaimsProvider claimsProvider, IRentalUnitRepository rentalUnitRepository)
        {
            _appDbContext = appDbContext;
            _rentalCaches = rentalCaches;
            _availabilityGuard = availabilityGuard;
            _invoicedOrderLinesValidator = invoicedOrderLinesValidator;
            _priceProfilesValidator = priceProfilesValidator;
            _claimsProvider = claimsProvider;
            _rentalUnitRepository = rentalUnitRepository;
        }

        public Task ValidateOrderAndInitializeCaches(Order order)
        {
            return ValidateOrderAndInitializeCaches(new List<Order> {order});
        }

        public async Task ValidateOrderAndInitializeCaches(List<Order> orders)
        {
            var subOrders = orders.SelectMany(x => x.SubOrders).ToList();
            var orderDateInterval = subOrders.DateInterval();
            var allocationOrderLines = subOrders.SelectMany(x => x.AllocationOrderLines).ToList();

            orders.ForEach(ValidateAllocationsOccupancy);

            var notPersistedAllocations = GetNotPersistedAllocations();
            var failedAllocation = await _availabilityGuard.AreUnitsGloballyAvailableForAllocations(notPersistedAllocations);

            if (failedAllocation != null) throw new AvailabilityException($"Enhed {failedAllocation.RentalUnitName} er ikke ledig i hele perioden {failedAllocation.Start.ToDanishDate()} - {failedAllocation.End.ToDanishDate()}");
            subOrders.Where(x => x.TenantId == Guid.Empty).ToList().ForEach(x => throw new CreateOrderException("SubOrder.TenantId is empty"));
            subOrders.SelectMany(x => x.OrderLines).Where(x => x.TenantId == Guid.Empty).ToList().ForEach(x => throw new CreateOrderException("TenantId missing from orderLine " + x.Text));
            allocationOrderLines.Where(x => x.PriceProfileId == Guid.Empty).ToList().ForEach(x => throw new CreateOrderException("PriceProfileId missing from allocationOrderLine " + x.Text));
            allocationOrderLines.Where(x => x.ResourceId != x.Allocation.RentalUnitId).ToList().ForEach(x => throw new CreateOrderException("OrderLine.ResourceId is different from Allocation.RentalUnitId " + x.Text));
            allocationOrderLines.Where(x => x.Start != x.Allocation.Start).ToList().ForEach(x => throw new CreateOrderException($"Orderline {x.Text} {x.Start.ToDanishDate()} does not match allocation startDate {x.Allocation.Start.ToDanishDate()}"));
            allocationOrderLines.Where(x => x.End != x.Allocation.End).ToList().ForEach(x => throw new CreateOrderException($"Orderline {x.Text} {x.Start.ToDanishDate()} does not match allocation endDate {x.Allocation.End.ToDanishDate()}"));
            allocationOrderLines.Where(x => x.Number != 0 - x.Allocation.Number).ToList().ForEach(x => throw new CreateOrderException($"Orderline {x.Text} number {x.Number}  does not match allocation number {0 - x.Allocation.Number}"));

            await InitializeRentalCaches(orderDateInterval, notPersistedAllocations);
            foreach (var order in orders)
            {
                await ValidateInvoicedOrderLines(order);
            }
            //await ValidatePriceProfilesOrderLines(order);
        }

        private void ValidateAllocationsOccupancy(Order order)
        {
            foreach (var subOrder in order.SubOrders)
            {
                var groupedAllocationOrderLines = subOrder.AllocationOrderLines.GroupBy(x => x.ResourceId);
                foreach (var groupedAllocationOrderLine in groupedAllocationOrderLines)
                {
                    AvailablityArray availabilityArray = new AvailablityArray(subOrder.Start, subOrder.End);

                    var allocationOrderLines = groupedAllocationOrderLine.Where(x => !x.IsEqualized);
                    availabilityArray.AddAllocations(allocationOrderLines.Select(x => x.Allocation).ToCacheAllocations());
                    var availability = availabilityArray.GetAvailability(subOrder.Start, subOrder.End);

                    //remember, the "base" allocation 2018-2050 is not included here
                    if (availability > 0) throw new AvailabilityOverReleasedException(groupedAllocationOrderLine.Key, _rentalUnitRepository.TryGetRentalUnitName(groupedAllocationOrderLine.Key));
                    if (availability < -1) throw new AvailabilityException($"Enhed {groupedAllocationOrderLine.Key} er ikke ledig i hele perioden {subOrder.Start.ToDanishDate()} - {subOrder.End.ToDanishDate()} for tenant {order.TenantId}");
                }
            }
        }

        private async Task InitializeRentalCaches(DateInterval orderDateInterval, List<Allocation> notPersistedAllocations)
        {
            if (orderDateInterval != null
            ) // orderDateInterval can be null if no suborder exists on the booking (e.g suborder is moved to another customer)
                await _rentalCaches.Initialize(orderDateInterval.Start, orderDateInterval.End);
            await _rentalCaches.AddAllocationsAndSave(notPersistedAllocations);
            await _appDbContext.SaveChangesAsync();
        }

        private List<Allocation> GetNotPersistedAllocations()
        {
            var notPersistedAllocations = _appDbContext.ChangeTracker.Entries().Where(e => e.State == EntityState.Added).Select(x => x.Entity).OfType<Allocation>().ToList();
            return notPersistedAllocations;
        }

        private async Task ValidateInvoicedOrderLines(Order order)
        {
            await this._invoicedOrderLinesValidator.ValidateInvoicedOrderLines(order);
        }

        private async Task ValidatePriceProfilesOrderLines(Order order)
        {
            var uniqPriceProfileIds = order.SubOrders.SelectMany(x => x.OrderLines).Where(x => x.PriceProfileId != Guid.Empty).Select(x => x.PriceProfileId).Distinct().ToList();
            var missingPriceProfileIds = await this._priceProfilesValidator.ValidatePriceProfileIds(uniqPriceProfileIds, _claimsProvider.GetTenantId());

            if (missingPriceProfileIds.Count > 0)
            {
                var orderLine = order.SubOrders.SelectMany(x => x.OrderLines).First(x => x.PriceProfileId == missingPriceProfileIds[0]);
                throw new ValidateOrderException($"PrisProfil {missingPriceProfileIds[0]}, {orderLine.PriceProfileName} {orderLine.Text} findes ikke");
            }
        }
    }
}
