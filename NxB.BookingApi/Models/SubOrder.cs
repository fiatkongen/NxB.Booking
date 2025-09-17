using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Munk.AspNetCore;
using Munk.Utils.Object;
using NxB.Allocating.Shared.Infrastructure;
using NxB.BookingApi.Models;
using NxB.BookingApi.Models.Exceptions;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Model;
using NxB.BookingApi.Exceptions;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class SubOrder : SubOrderArticle
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public DateInterval DateInterval
        {
            get => new(Start, End);
            set
            {
                if (value != null)
                {
                    Start = value.Start;
                    End = value.End;
                }
            }
        }

        public List<ResourceBasedOrderLine> ResourceBasedOrderLines => OrderLines.OfType<ResourceBasedOrderLine>().ToList();
        public List<TimedBasedOrderLine> TimedBasedOrderLines => OrderLines.OfType<TimedBasedOrderLine>().ToList();

        public List<AllocationOrderLine> AllocationOrderLines
        {
            get => OrderLines.OfType<AllocationOrderLine>().ToList();
            private set => OrderLines.AddRange(value);  //used by mapper
        }

        public List<AllocationOrderLine> NotPersistedAllocationOrderLines
        {
            get { return AllocationOrderLines.Where(x => x.Id == Guid.Empty).ToList(); }
        }

        public List<GuestOrderLine> GuestOrderLines
        {
            get => OrderLines.OfType<GuestOrderLine>().ToList();
            private set => OrderLines.AddRange(value);  //used by mapper
        }

        public List<GuestOrderLine> NotPersistedGuestOrderLines
        {
            get { return this.GuestOrderLines.Where(x => x.Id == Guid.Empty).ToList(); }
        }

        public IList<Allocation> Allocations => AllocationOrderLines.Select(x => x.Allocation).ToList();

        public TimedBasedOrderLine CreateTimeBasedOrderLine(string orderLineType, Guid createAuthorId, decimal number, DateInterval dateInterval, Guid resourceId, string text, Guid priceProfileId, string priceProfileName, decimal pricePcs, decimal tax, int index = 0)
        {
            if (orderLineType == typeof(AllocationOrderLine).Name)
            {
                return CreateAllocationOrderLine(createAuthorId, number, dateInterval, resourceId, text, priceProfileId, priceProfileName, pricePcs, tax, false, index);
            }
            if (orderLineType == typeof(GuestOrderLine).Name)
            {
                return CreateGuestOrderLine(createAuthorId, number, dateInterval, resourceId, text, priceProfileId, priceProfileName, pricePcs, tax, index);
            }

            throw new ArgumentException("Could not create timebased orderline of type " + orderLineType);
        }

        public AllocationOrderLine CreateAllocationOrderLine(Guid createAuthorId, decimal number, DateInterval dateInterval, Guid resourceId, string text, Guid priceProfileId, string priceProfileName, decimal pricePcs, decimal tax, bool isCustomPricePcs, int index = 0)
        {
            var allocationOrderLine = new AllocationOrderLine
            {
                Id = new Guid(),
                TenantId = TenantId,
                CreateAuthorId = createAuthorId,
                CreateDate = DateTime.Now.ToEuTimeZone(),
                Interval = dateInterval,
                Number = number,
                Index = index,
                ResourceId = resourceId,
                Tax = tax,
                Text = text,
                PriceProfileId = priceProfileId,
                PriceProfileName = priceProfileName,
                PricePcs = pricePcs,
                SubOrder = this,
                SubOrderId = this.Id,
                IsCustomPricePcs = isCustomPricePcs
            };

            return allocationOrderLine;
        }

        public AllocationOrderLine CreateAllocationOrderLineWithAllocation(Guid createAuthorId, decimal number,
            DateInterval dateInterval, Guid resourceId, string text, Guid priceProfileId, string priceProfileName,
            decimal pricePcs, decimal tax, bool isCustomPricePcs, int index = 0)
        {
            var orderline = CreateAllocationOrderLine(createAuthorId, number, dateInterval, resourceId, text, priceProfileId, priceProfileName, pricePcs, tax, isCustomPricePcs, index);
            Allocation allocation = new Allocation(Guid.NewGuid(), this.TenantId, resourceId, text, dateInterval, 0 - number);
            orderline.Allocation = allocation;
            return orderline;
        }

        public GuestOrderLine CreateGuestOrderLine(Guid createAuthorId, decimal number, DateInterval dateInterval, Guid resourceId, string text, Guid priceProfileId, string priceProfileName, decimal pricePcs, decimal tax, int index = 0)
        {
            var guestOrderLine = new GuestOrderLine
            {
                Id = new Guid(),
                TenantId = TenantId,
                CreateAuthorId = createAuthorId,
                CreateDate = DateTime.Now.ToEuTimeZone(),
                Interval = dateInterval,
                Number = number,
                Index = index,
                ResourceId = resourceId,
                Tax = tax,
                Text = text,
                PriceProfileId = priceProfileId,
                PriceProfileName = priceProfileName,
                PricePcs = pricePcs,
                SubOrder = this,
                SubOrderId = this.Id
            };

            return guestOrderLine;
        }

        public override void EqualizeOrderLines()
        {
            var revertedLineIds = ResourceBasedOrderLines.Where(x => x.RevertedLineId.HasValue)
                .Select(x => x.RevertedLineId).ToList();

            var revertedLines = this.ResourceBasedOrderLines.Where(x => revertedLineIds.Contains(x.Id)).ToList();
            foreach (var revertedLine in revertedLines)
            {
                var matchingRevertingLine = ResourceBasedOrderLines.SingleOrDefault(x =>
                {
                    return !x.IsEqualized &&
                           revertedLine.Id == x.RevertedLineId &&
                           revertedLine.Number == 0 - x.Number &&
                           revertedLine.ResourceId == x.ResourceId &&
                           revertedLine.PriceProfileId == x.PriceProfileId &&
                           revertedLine.PriceProfileName == x.PriceProfileName &&
                           revertedLine.PricePcs == x.PricePcs
                           && (
                               !(revertedLine is TimedBasedOrderLine) || !(x is TimedBasedOrderLine) || ((TimedBasedOrderLine)x).Interval == ((TimedBasedOrderLine)revertedLine).Interval
                           );
                });

                if (matchingRevertingLine == null)
                    throw new RevertOrderLineException(revertedLine, "Kunne ikke finde matchende linie");

                revertedLine.Equalize();
                matchingRevertingLine.Equalize();
            }
            base.EqualizeOrderLines();
            UpdateDateInterval();
        }

        public void UpdateDateInterval()
        {
            var groupedCacheAllocations = GroupCachedAllocationsByResourceId(this.AllocationOrderLines);
            DateInterval firstDateInterval = null;

            foreach (var groupedCacheAllocation in groupedCacheAllocations)
            {
                var cacheAllocations = groupedCacheAllocation.Value;
                var dateInterval = CalculateDateInterval(cacheAllocations);

                if (dateInterval != null)
                {
                    if (firstDateInterval == null)
                    {
                        firstDateInterval = dateInterval;
                    }
                    else
                    {
                        if (firstDateInterval != dateInterval)
                        {
                            throw new AddSubOrdersException($"Der findes flere datointervaller for enheder på denne reservation ({dateInterval}, {this.DateInterval}). Dette er ikke muligt.");
                        }
                    }
                }
            }

            if (firstDateInterval == null)
            {
                //Try to allow this
                //just keep the "old" dateinterval
                // throw new AddSubOrdersException($"Der findes ingen datointervaller for enheder på denne reservation. Ingen enheder er reserveret. Dette er ikke muligt.");
            }
            else
            {
                this.DateInterval = firstDateInterval;
            }
        }

        private Dictionary<Guid, List<CacheAllocation>> GroupCachedAllocationsByResourceId(IEnumerable<TimedBasedOrderLine> timeBasedOrderLines)
        {
            return timeBasedOrderLines.Where(x => !x.IsEqualized).GroupBy(x => x.ResourceId, x => x, (key, a) => new { resourceId = key, cacheAllocations = a.Select(x => new CacheAllocation(x.ResourceId.ToString(), x.Start, x.End, x.Number)).ToList() }).ToDictionary(x => x.resourceId, x => x.cacheAllocations);
        }

        private DateInterval CalculateDateInterval(List<CacheAllocation> cacheAllocations)
        {
            var widestDateInterval = new DateInterval(cacheAllocations.Min(x => x.Start), cacheAllocations.Max(x => x.End));
            AvailablityArray availabilityArray = new AvailablityArray(widestDateInterval.Start, widestDateInterval.End);
            availabilityArray.AddAllocations(cacheAllocations.ToArray());

            var availability = availabilityArray.GetAvailabilityArray(widestDateInterval.Start, widestDateInterval.End);
            if (availability.All(x => x == 0))
            {
                return null;
            }

            var latestStartDate = CalculateAvailabilityStartDate(availability, widestDateInterval.Start);
            var earliestEndDate = CalculateAvailabilityEndDate(availability, widestDateInterval.End);
            widestDateInterval = new DateInterval(latestStartDate, earliestEndDate);

            var availabilityCount = availabilityArray.GetAvailability(widestDateInterval.Start, widestDateInterval.End);
            availability = availabilityArray.GetAvailabilityArray(widestDateInterval.Start, widestDateInterval.End);

            if (availabilityCount <= 1 && cacheAllocations.Any())
            {
                var resourceId = cacheAllocations.First().ResourceId;
                var resourceName = this.TimedBasedOrderLines.First(x => x.ResourceId.ToString() == resourceId).Text;

                if (availabilityCount < 0)
                {
                    throw new AvailabilityOverReleasedException(Guid.Parse(resourceId), resourceName);
                }

                if (availabilityCount < 1)
                {
                    throw new AddSubOrdersException(resourceName + " er ikke booket hele perioden " + widestDateInterval);
                }
            }

            var startDate = widestDateInterval.Start;
            var endDate = widestDateInterval.End;

            startDate = CalculateAvailabilityStartDate(availability, startDate);

            if (startDate == endDate) return null;

            endDate = CalculateAvailabilityEndDate(availability, endDate);

            return new DateInterval(startDate, endDate);
        }

        private static DateTime CalculateAvailabilityEndDate(decimal[] availability, DateTime endDate)
        {
            for (int i = availability.Length - 1; i >= 0; i--)
            {
                if (availability[i] != 0) break;
                endDate = endDate.AddDays(-1);
            }

            return endDate;
        }

        private static DateTime CalculateAvailabilityStartDate(decimal[] availability, DateTime startDate)
        {
            for (int i = 0; i < availability.Length; i++)
            {
                if (availability[i] != 0) break;
                startDate = startDate.AddDays(1);
            }

            return startDate;
        }

        public async Task<List<TimedBasedOrderLine>> CalculateDateGapsForTimeBasedOrderLines(DateInterval newDateInterval, Guid createAuthorId)
        {
            var timedBasedOrderLines = new List<TimedBasedOrderLine>();

            if (newDateInterval.Start < DateInterval.Start)
            {
                var borderDateInterval = new DateInterval(DateInterval.Start, DateInterval.Start.AddDays(1));
                var dateInterval = new DateInterval(newDateInterval.Start, DateInterval.Start);
                timedBasedOrderLines.AddRange(CreateExtendedTimeBasedOrderLines(createAuthorId, dateInterval, borderDateInterval));
            }

            if (newDateInterval.Start > DateInterval.Start)
            {
                var dateInterval = new DateInterval(DateInterval.Start, newDateInterval.Start);
                timedBasedOrderLines.AddRange(CreateShrinkedTimeBasedOrderLines(createAuthorId, dateInterval));
            }

            if (newDateInterval.End > DateInterval.End)
            {
                var borderDateInterval = new DateInterval(DateInterval.End.AddDays(-1), DateInterval.End);
                var dateInterval = new DateInterval(DateInterval.End, newDateInterval.End);
                timedBasedOrderLines.AddRange(CreateExtendedTimeBasedOrderLines(createAuthorId, dateInterval, borderDateInterval));
            }

            if (newDateInterval.End < DateInterval.End)
            {
                var dateInterval = new DateInterval(newDateInterval.End, DateInterval.End);
                timedBasedOrderLines.AddRange(CreateShrinkedTimeBasedOrderLines(createAuthorId, dateInterval));
            }

            return timedBasedOrderLines;
        }

        private List<TimedBasedOrderLine> CreateExtendedTimeBasedOrderLines(Guid createAuthorId, DateInterval dateInterval, DateInterval borderDateInterval)
        {
            var groupedCacheAllocations = GroupCachedAllocationsByResourceAndPriceProfile(this.TimedBasedOrderLines);
            var timedBasedOrderLines = new List<TimedBasedOrderLine>();

            var widestDateInterval = new DateInterval(
                groupedCacheAllocations.Values.SelectMany(x => x).Min(x => x.Start),
                groupedCacheAllocations.Values.SelectMany(x => x).Max(x => x.End));

            foreach (var groupedCacheAllocation in groupedCacheAllocations)
            {
                AvailablityArray workingAvailabilityArray = new AvailablityArray(widestDateInterval.Start, widestDateInterval.End);
                workingAvailabilityArray.AddAllocations(groupedCacheAllocation.Value.ToArray());
                var borderOccCount = workingAvailabilityArray.GetAvailability(borderDateInterval.Start, borderDateInterval.End);

                if (borderOccCount > 0)
                {
                    var guestOrderLine = CreateTimeBasedOrderLine(
                        groupedCacheAllocation.Key.orderLineType,
                        createAuthorId,
                        borderOccCount,
                        dateInterval,
                        groupedCacheAllocation.Key.resourceId,
                        groupedCacheAllocation.Key.resourceName,
                        groupedCacheAllocation.Key.priceProfileId,
                        groupedCacheAllocation.Key.priceProfileName,
                        0,
                        groupedCacheAllocation.Key.tax);
                    timedBasedOrderLines.Add(guestOrderLine);
                }
            }

            return timedBasedOrderLines;
        }

        private List<TimedBasedOrderLine> CreateShrinkedTimeBasedOrderLines(Guid createAuthorId, DateInterval dateInterval)
        {
            var groupedCacheAllocations = GroupCachedAllocationsByResourceAndPriceProfile(this.TimedBasedOrderLines);
            var timedBasedOrderLines = new List<TimedBasedOrderLine>();

            var widestDateInterval = new DateInterval(
                groupedCacheAllocations.Values.SelectMany(x => x).Min(x => x.Start),
                groupedCacheAllocations.Values.SelectMany(x => x).Max(x => x.End));

            foreach (var groupedCacheAllocation in groupedCacheAllocations)
            {
                AvailablityArray workingAvailabilityArray = new AvailablityArray(widestDateInterval.Start, widestDateInterval.End);
                workingAvailabilityArray.AddAllocations(groupedCacheAllocation.Value.ToArray());
                var allocations = workingAvailabilityArray.GenerateAvailabilityCacheAllocations(groupedCacheAllocation.Key.resourceId.ToString(), dateInterval.Start, dateInterval.End);

                foreach (var allocation in allocations)
                {
                    var guestOrderLine = CreateTimeBasedOrderLine(
                        groupedCacheAllocation.Key.orderLineType,
                        createAuthorId,
                        0 - allocation.Number,
                        new DateInterval(allocation.Start, allocation.End),
                        groupedCacheAllocation.Key.resourceId,
                        groupedCacheAllocation.Key.resourceName,
                        groupedCacheAllocation.Key.priceProfileId,
                        groupedCacheAllocation.Key.priceProfileName,
                        0,
                        groupedCacheAllocation.Key.tax);
                    timedBasedOrderLines.Add(guestOrderLine);
                }
            }

            return timedBasedOrderLines;
        }

        private Dictionary<(string orderLineType, Guid resourceId, string resourceName, Guid priceProfileId, string priceProfileName, decimal tax), List<CacheAllocation>> GroupCachedAllocationsByResourceAndPriceProfile(IEnumerable<TimedBasedOrderLine> timeBasedOrderLines)
        {
            return timeBasedOrderLines.Where(x => !x.IsEqualized)
                .GroupBy(x => (orderLineType: x.GetType().Name, resourceId: x.ResourceId, resourceName: x.Text, priceProfileId: x.PriceProfileId, priceProfileName: x.PriceProfileName, tax: x.Tax), x => x,
                    (key, orderLines) =>
                    {
                        var timedBasedOrderLines = orderLines.ToList();
                        return new
                        {
                            orderLineType = key.orderLineType,
                            resourceId = key.resourceId,
                            resourceName = key.resourceName,
                            priceProfileId = key.priceProfileId,
                            priceProfileName = key.priceProfileName,
                            tax = key.tax,
                            cacheAllocations = timedBasedOrderLines.Select(x =>
                                new CacheAllocation(x.ResourceId.ToString(), x.Start, x.End, x.Number)).ToList(),
                            orderLine = timedBasedOrderLines
                        };
                    })
                .ToDictionary(x => (x.orderLineType, x.resourceId, x.resourceName, x.priceProfileId, x.priceProfileName, x.tax), x => x.cacheAllocations);
        }

        public override SubOrderSection AugmentSubOrderSectionsAndOrderLines(Guid createAuthorId, Guid tenantId)
        {
            SubOrderSection subOrderSection = base.AugmentSubOrderSectionsAndOrderLines(createAuthorId, tenantId);

            NotPersistedAllocationOrderLines.ForEach(line =>
            {
                line.Id = Guid.NewGuid();
                line.TenantId = TenantId;
                line.CreateAuthorId = createAuthorId;
                line.SubOrderId = Id;
                line.SubOrder = this;
                line.AllocationId = line.Allocation.Id;
                line.Allocation.TenantId = tenantId;
                line.SubOrderSectionId = subOrderSection.Id;
                line.SubOrderSection = subOrderSection;
                subOrderSection.OrderLines.Add(line);
            });
            NotPersistedGuestOrderLines.ForEach(line =>
            {
                line.Id = Guid.NewGuid();
                line.TenantId = TenantId;
                line.CreateAuthorId = createAuthorId;
                line.SubOrderId = Id;
                line.SubOrder = this;
                line.SubOrderSectionId = subOrderSection.Id;
                line.SubOrderSection = subOrderSection;
                subOrderSection.OrderLines.Add(line);
            });
            return subOrderSection;
        }

        internal void Revert(Guid createAuthorId, Guid tenantId)
        {
            var orderLines = this.OrderLines.Where(x => !x.IsEqualized).ToList();
            if (orderLines.Count == 0) throw new RevertSubOrderException(this, "Der er ikke nogen linier at tilbageføre");

            this.OrderLines.AddRange(orderLines.Select(x => x.Revert(createAuthorId)).ToList());
            this.AugmentSubOrderSectionsAndOrderLines(createAuthorId, tenantId);
            this.EqualizeOrderLines();
        }

        internal void RevertOrderLine(Guid createAuthorId, Guid orderLineId)
        {
            var orderLine = this.OrderLines.SingleOrDefault(x => !x.IsEqualized && x.Id == orderLineId);
            if (orderLine == null) throw new RevertSubOrderException(this, "Der er ikke nogen linie at tilbageføre");

            this.OrderLines.Add(orderLine.Revert(createAuthorId));
        }

        internal void Release(Guid createAuthorId, Guid tenantId)
        {
            var allocationOrderLines = this.AllocationOrderLines.Where(x => !x.IsEqualized && x.Number != 0).ToList();

            if (allocationOrderLines.Count == 0) throw new RevertSubOrderException(this, "Der er ikke nogen linier med optagede enheder at frigive");

            var minDate = allocationOrderLines.Min(x => x.Start);
            var maxDate = allocationOrderLines.Max(x => x.End);
            AvailablityArray workingAvailabilityArray = new AvailablityArray(minDate, maxDate);
            workingAvailabilityArray.AddAllocations(allocationOrderLines.Select(x => x.Allocation).ToCacheAllocations());

            var allocationOrderLine = allocationOrderLines[0];
            var resourceId = allocationOrderLine.ResourceId;
            var intervalsToBeReleased = workingAvailabilityArray.GenerateAvailabilityCacheAllocations(resourceId.ToString(), minDate, maxDate);

            if (intervalsToBeReleased.Count == 0)
            {
                throw new AvailabilityOverReleasedException(resourceId, allocationOrderLines[0].Text);
            }
            if (intervalsToBeReleased.Count > 1)
            {
                throw new AvailabilityException(
                    $"Kan ikke frigøre enhed {resourceId}. intervalToBeReleased.Count={intervalsToBeReleased.Count}");
            }

            var intervalToBeReleased = intervalsToBeReleased[0];
            this.OrderLines.Add(CreateAllocationOrderLineWithAllocation(createAuthorId, intervalToBeReleased.Number, new DateInterval(intervalToBeReleased.Start, intervalToBeReleased.End), resourceId, allocationOrderLine.Text, allocationOrderLine.PriceProfileId, allocationOrderLine.PriceProfileName, 0, 0, true, (int)allocationOrderLines.Last().Index));
            this.AugmentSubOrderSectionsAndOrderLines(createAuthorId, tenantId);
        }

        public List<OrderLine> BuildUnRevertedOrderLines(Guid createAuthorId)
        {
            var orderLines = GetRevertedSection().BuildUnRevertedOrderLines(createAuthorId);
            orderLines.ForEach(x => x.Index = 0 - x.Index);
            return orderLines;
        }

        public SubOrderSection GetRevertedSection()
        {
            if (!IsEqualized)
                throw new CopyRevertedSubOrderException(this, "Del-ordre er ikke tilbageført.");

            return this.SubOrderSections.OrderBy(x => x.Index).Last();
        }

        public void MarkSubOrderDiscountsAsDeleted(List<Guid> subOrderDiscountIds)
        {
            this.SubOrderDiscounts.Where(x => subOrderDiscountIds.Any(sudi => sudi == x.Id)).ToList().ForEach(x => x.MarkAsDeleted());
        }
    }
}