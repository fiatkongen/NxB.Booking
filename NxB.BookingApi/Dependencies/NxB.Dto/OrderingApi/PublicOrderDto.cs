using NxB.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.OrderingApi
{
    public class PublicOrderDto
    {
        public long FriendlyId { get; set; }
        public List<PublicSubOrderDto> SubOrders { get; set; }
    }

    public class PublicSubOrderDto
    {
        public Guid Id { get; set; }
        public int Index { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public List<SubOrderSectionDto> SubOrderSections { get; set; }

        public List<AllocationStateDto> AllocationStates { get; set; }
        public AllocationStateDto AllocationState { get; set; }

        public List<AllocationOrderLineDto> AllocationOrderLines { get; set; } = new();
        public List<GuestOrderLineDto> GuestOrderLines { get; set; } = new();
        public List<ArticleOrderLineDto> ArticleOrderLines { get; set; } = new();
        public List<DiscountOrderLineDto> DiscountOrderLines { get; set; } = new();
        public List<SubOrderDiscountLineDto> SubOrderDiscountLines { get; set; } = new();
        public List<SubOrderDiscountDto> SubOrderDiscounts { get; set; } = new();
        public List<OrderLineDto> OrderLines => this.AllocationOrderLines.Cast<OrderLineDto>().Concat(GuestOrderLines).Concat(ArticleOrderLines).Concat(DiscountOrderLines).Concat(SubOrderDiscountLines).ToList();

        public bool? IsOnSite => AllocationState == null ? null : (
            AllocationState.ArrivalStatus == ArrivalStatus.Arrived &&
            (AllocationState.DepartureStatus == DepartureStatus.NotDeparted || AllocationState.DepartureStatus == DepartureStatus.DelayedDeparture));
    }
}
