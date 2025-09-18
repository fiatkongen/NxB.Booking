using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using Munk.Utils.Object;
using Newtonsoft.Json;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;

namespace NxB.Dto.OrderingApi
{
    public class OrderDto : IOrderKey
    {
        [Required]
        [NoEmpty]
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }

        public DateTime? CreateDate { get; set; }
        public DateTime LastEditDate { get; set; }

        [Required]
        [NoEmpty]
        public Guid AccountId { get; set; }

        public long FriendlyId { get; set; }
        public List<SubOrderDto> SubOrders { get; set; }

        public bool NoteState { get; set; }
        public decimal? LastSumTotal { get; set; }

        //used only for testing
        public decimal CalculateTotal()
        {
            return this.SubOrders.SelectMany(x => x.OrderLines.Where(ol => !ol.IsEqualized))
                .Sum(ol => ol.PricePcs * ol.Number);
        }

        public string Note { get; set; }

        public CreatedBy CreatedBy { get; set; }
        public string CreatedByExternalId { get; set; }
        public string CreateNote { get; set; }
        public string OnlineTransactionDetails { get; set; }
        public string ExternalId { get; set; }
    }

    public class CreateOrderDto
    {
        [Required]
        [NoEmpty]
        public Guid AccountId { get; set; }
        public long? OverrideFriendlyId { get; set; }
        public DateTime? ImportTimeStamp { get; set; }
        public string Note { get; set; }
        public List<CreateSubOrderDto> SubOrders { get; set; }
        public List<SubOrderDto> ExistingSubOrders { get; set; }
        public CreatedBy CreatedBy { get; set; }
        public string CreateNote { get; set; }
    }

    public class AddToOrderDto
    {
        [Required]
        [NoEmpty]
        public Guid Id { get; set; }
        public List<CreateOrAddToSubOrderDto> SubOrders { get; set; }
    }

    public class SubOrderSectionDto
    {
        public Guid Id { get; set; }
        public Guid CreateAuthorId { get; set; }
        public string CreateAuthorName { get; set; }
        public DateTime CreateDate { get; set; }
        public int Index { get; set; }
    }

    public class CreateSubOrderDiscountDto
    {
        public Guid Id { get; set; }
        public Guid DiscountId { get; set; }
        public decimal DiscountPercent { get; set; }
        public string Text { get; set; }
        public int Index { get; set; }
    }

    public class SubOrderDiscountDto : CreateSubOrderDiscountDto
    {
        public bool IsDeleted { get; set; }
    }

    public class SubOrderDto
    {
        public Guid Id { get; set; }
        public int Index { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool IsEqualized { get; set; }
        public string Note { get; set; }
        public bool NoteState { get; set; }

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

    public class CreateSubOrderDto : CreateSubOrderArticleDto
    {
        public List<CreateAllocationOrderLineDto> AllocationOrderLines { get; set; } = new();
        public List<CreateGuestOrderLineDto> GuestOrderLines { get; set; } = new();
        public List<Guid> SubOrderDiscountIdsMarkedForDeletion { get; set; } = new();
    }

    public class CreateOrAddToSubOrderDto : CreateSubOrderDto
    {
        public Guid? Id { get; set; }
    }

    public class CreateSubOrderArticleDto
    {
        public int Index { get; set; }
        public string Note { get; set; }
        public List<CreateArticleOrderLineDto> ArticleOrderLines { get; set; } = new();
        public List<CreateDiscountOrderLineDto> DiscountOrderLines { get; set; } = new();
        public List<CreateSubOrderDiscountLineDto> SubOrderDiscountLines { get; set; } = new();

        public List<CreateSubOrderDiscountDto> SubOrderDiscounts { get; set; } = new();
    }

    public class CreateOrAddToSubOrderArticleDto : CreateSubOrderArticleDto
    {
        public Guid? Id { get; set; }
    }

    public class OrderLineDto
    {
        [Required]
        [NoEmpty]
        public Guid Id { get; set; }

        [Required]
        [NoEmpty]
        public Guid CreateAuthorId { get; set; }
        public DateTime CreateDate { get; set; }

        public decimal Number { get; set; }
        public decimal PricePcs { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal Index { get; set; }
        public string CreateAuthorName { get; set; }
        public bool IsEqualized { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Text { get; set; }

        [Required]
        [NoEmpty]
        public Guid PriceProfileId { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PriceProfileName { get; set; }

        [Required]
        [NoEmpty]
        public Guid SubOrderSectionId { get; set; }

        public bool IsCustomPricePcs { get; set; }

        public DateTime? OriginalCreateDate { get; set; }
    }

    public abstract class ResourceBasedOrderLineDto : OrderLineDto
    {
        public Guid ResourceId { get; set; }
    }

    public abstract class TimeBasedOrderLineDto : ResourceBasedOrderLineDto
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    public class AllocationOrderLineDto : TimeBasedOrderLineDto
    {
        public Guid AllocationId { get; set; }
        public Guid? RentalSubTypeId { get; set; }
    }

    public class GuestOrderLineDto : TimeBasedOrderLineDto
    {
    }

    public class ArticleOrderLineDto : ResourceBasedOrderLineDto
    {
        public bool IsMetered => MeterReference.HasValue;
        public decimal? MeterStart { get; set; }
        public decimal? MeterEnd { get; set; }
        public Guid? MeterReference { get; set; }
    }

    public class DiscountOrderLineDto : ResourceBasedOrderLineDto
    {
    }

    public class SubOrderDiscountLineDto : DiscountOrderLineDto
    {
        public Guid SubOrderDiscountId { get; set; }
        public decimal DiscountPercent { get; set; }
    }

    public class CreateOrderLineDto
    {
        public decimal Number { get; set; }
        public decimal SuggestedPricePcs { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Text { get; set; }

        [Required]
        [NoEmpty]
        public Guid PriceProfileId { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PriceProfileName { get; set; }

        public decimal Index { get; set; }
        public Guid? RevertedLineId { get; set; }
        public bool IsCustomPricePcs { get; set; }
    }

    public abstract class CreateResourceBasedOrderLineDto : CreateOrderLineDto
    {
        [Required]
        [NoEmpty]
        public Guid ResourceId { get; set; }
    }

    public abstract class CreateTimedBasedOrderLineDto : CreateResourceBasedOrderLineDto
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    public class CreateGuestOrderLineDto : CreateTimedBasedOrderLineDto
    {
    }

    public class CreateAllocationOrderLineDto : CreateTimedBasedOrderLineDto
    {
        public Guid? RentalSubTypeId { get; set; }
    }

    public class CreateArticleOrderLineDto : CreateResourceBasedOrderLineDto
    {
        public decimal? MeterStart { get; set; }
        public decimal? MeterEnd { get; set; }
        public Guid? MeterReference { get; set; }
        public DateTime? OriginalCreateDate { get; set; }
    }

    public class CreateDiscountOrderLineDto : CreateResourceBasedOrderLineDto
    {
    }

    public class CreateSubOrderDiscountLineDto : CreateResourceBasedOrderLineDto
    {
        [Required]
        [NoEmpty]
        public Guid SubOrderDiscountId { get; set; }
        public decimal DiscountPercent { get; set; }
    }

    public class TimeBasedOrderLinesDto
    {
        public List<AllocationOrderLineDto> AllocationOrderLines { get; set; } = new();
        public List<GuestOrderLineDto> GuestOrderLines { get; set; } = new();
    }

    public class OrderLinesDto : TimeBasedOrderLinesDto
    {
        public List<ArticleOrderLineDto> ArticleOrderLines { get; set; } = new();
        public List<DiscountOrderLineDto> DiscountOrderLines { get; set; } = new();
        public List<SubOrderDiscountLineDto> SubOrderDiscountLines { get; set; } = new();

        [JsonIgnore]
        public List<OrderLineDto> OrderLines =>
            this.AllocationOrderLines.Cast<OrderLineDto>().Concat(this.GuestOrderLines).Concat(this.ArticleOrderLines).Concat(this.DiscountOrderLines).Concat(this.SubOrderDiscountLines).ToList();
    }

    public class ModifySubOrderNoteDto
    {
        public Guid SubOrderId { get; set; }
        public string Note { get; set; }
        public bool? NoteState { get; set; } = null;
    }

    public class ModifyOrderNoteDto
    {
        public Guid OrderId { get; set; }
        public string Note { get; set; }
        public bool? NoteState { get; set; } = null;
    }

    public class ModifyOrderOnlineTransactionDetails
    {
        public Guid OrderId { get; set; }
        public string TransactionDetails { get; set; } = null;
    }
}
