using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Munk.AspNetCore;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;
using NxB.BookingApi.Exceptions;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public abstract class 
        OrderLine : ICreateAudit, ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Guid CreateAuthorId { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now.ToEuTimeZone();
        public DateTime? OriginalCreateDate { get; set; }
        public decimal Number { get; set; }
        public decimal PricePcs { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxPercent { get; set; }
        public string Text { get; set; }
        public decimal Index { get; set; }
        public bool IsEqualized { get; private set; }
        public string PriceProfileName { get; set; }
        public Guid PriceProfileId { get; set; }
        public Guid? RevertedLineId { get; set; }
        public bool IsCustomPricePcs { get; set; }

        public Guid SubOrderId { get; set; }
        public SubOrderArticle SubOrder { get; set; }

        public Guid SubOrderSectionId { get; set; }
        public SubOrderSection SubOrderSection { get; set; }
        public decimal Total => Number * PricePcs;

        //Quantity? To solve problems of larger quantities


        public void Equalize()
        {
            this.IsEqualized = true;
        }

        public void RemoveEqualize()
        {
            this.IsEqualized = false;
            this.RevertedLineId = null;
        }

        public abstract OrderLine Revert(Guid createAuthorId);
        public abstract OrderLine Release(Guid createAuthorId);
        public abstract OrderLine Clone();

        public OrderLine UnRevertOrderLine(Guid createAuthorId)
        {
            var unrevertedOrderLine = this.Revert(createAuthorId);
            unrevertedOrderLine.RevertedLineId = null;
            unrevertedOrderLine.RevertedLineId = null;
            unrevertedOrderLine.SubOrderSection = null;
            unrevertedOrderLine.SubOrderSectionId = Guid.Empty;
            return unrevertedOrderLine;
        }
    }

    [Serializable]
    public abstract class ResourceBasedOrderLine : OrderLine, ITaxableItem
    {
        public Guid ResourceId { get; set; }
    }

    [Serializable]
    public abstract class TimedBasedOrderLine : ResourceBasedOrderLine
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public DateInterval Interval
        {
            get => new(Start, End);
            set
            {
                Start = value.Start;
                End = value.End;
            }
        }
    }

    [Serializable]
    public class AllocationOrderLine : TimedBasedOrderLine
    {
        public Guid AllocationId { get; set; }
        public Allocation Allocation { get; set; }

        public Guid? RentalSubTypeId { get; set; }

        public override OrderLine Revert(Guid createAuthorId)
        {
            decimal number = 0 - this.Number;
            Allocation allocation = new Allocation(Guid.NewGuid(), Guid.Empty, this.ResourceId, this.Text, this.Interval, 0 - this.Allocation.Number);

            AllocationOrderLine allocationOrderLine = new AllocationOrderLine
            {
                CreateAuthorId = createAuthorId,
                OriginalCreateDate = this.OriginalCreateDate,
                TenantId = TenantId,
                Number = number,
                PricePcs = this.PricePcs,
                Tax = 0 - this.Tax,
                TaxPercent = TaxPercent,
                Text = this.Text,
                PriceProfileId = this.PriceProfileId,
                PriceProfileName = this.PriceProfileName,
                RevertedLineId = this.Id,
                ResourceId = this.ResourceId,
                Interval = this.Interval,
                SubOrder = this.SubOrder,
                SubOrderId = this.SubOrderId,
                Allocation = allocation,
                AllocationId = allocation.Id,
                Index = Index,
                IsCustomPricePcs = IsCustomPricePcs,
                RentalSubTypeId = RentalSubTypeId
            };
            return allocationOrderLine;
        }

        public override OrderLine Release(Guid createAuthorId)
        {
            decimal number = 0 - this.Number;
            Allocation allocation = new Allocation(Guid.NewGuid(), Guid.Empty, this.ResourceId, this.Text, this.Interval, 0 - this.Allocation.Number);

            AllocationOrderLine allocationOrderLine = new AllocationOrderLine
            {
                CreateAuthorId = createAuthorId,
                TenantId = TenantId,
                Number = number,
                PricePcs = 0,
                Tax = 0,
                TaxPercent = TaxPercent,
                Text = this.Text,
                PriceProfileId = this.PriceProfileId,
                PriceProfileName = this.PriceProfileName,
                ResourceId = this.ResourceId,
                Interval = this.Interval,
                SubOrder = this.SubOrder,
                SubOrderId = this.SubOrderId,
                Allocation = allocation,
                AllocationId = allocation.Id,
                Index = Index,
                IsCustomPricePcs = true,
                RentalSubTypeId = RentalSubTypeId
            };

            return allocationOrderLine;
        }

        public override OrderLine Clone()
        {
            Allocation allocation = new Allocation(Guid.NewGuid(), Guid.Empty, this.ResourceId, this.Text, this.Interval, 0 -Number);

            AllocationOrderLine allocationOrderLine = new AllocationOrderLine
            {
                TenantId = TenantId,
                OriginalCreateDate = this.OriginalCreateDate,
                Number = Number,
                PricePcs = this.PricePcs,
                Tax = this.Tax,
                TaxPercent = this.TaxPercent,
                Text = this.Text,
                PriceProfileId = this.PriceProfileId,
                PriceProfileName = this.PriceProfileName,
                ResourceId = this.ResourceId,
                Interval = this.Interval,
                SubOrder = this.SubOrder,
                SubOrderId = this.SubOrderId,
                Allocation = allocation,
                Index = Index,
                IsCustomPricePcs = IsCustomPricePcs,
                RentalSubTypeId = RentalSubTypeId
            };

            return allocationOrderLine;
        }
    }

    [Serializable]
    public class GuestOrderLine : TimedBasedOrderLine
    {
        public override OrderLine Revert(Guid createAuthorId)
        {
            decimal number = 0 - this.Number;
            GuestOrderLine guestOrderLine = new GuestOrderLine
            {
                CreateAuthorId = createAuthorId,
                OriginalCreateDate = this.OriginalCreateDate,
                TenantId = TenantId,
                Number = number,
                PricePcs = this.PricePcs,
                Tax = this.Tax,
                Text = this.Text,
                PriceProfileId = this.PriceProfileId,
                PriceProfileName = this.PriceProfileName,
                RevertedLineId = this.Id,
                ResourceId = this.ResourceId,
                Interval = this.Interval,
                SubOrder = this.SubOrder,
                SubOrderId = this.SubOrderId,
                Index = Index,
                IsCustomPricePcs = IsCustomPricePcs
            };
            return guestOrderLine;
        }

        public override OrderLine Release(Guid createAuthorId)
        {
            return null;
        }

        public override OrderLine Clone()
        {
            GuestOrderLine guestOrderLine = new GuestOrderLine
            {
                Number = Number,
                OriginalCreateDate = this.OriginalCreateDate,
                TenantId = TenantId,
                PricePcs = this.PricePcs,
                Tax = this.Tax,
                TaxPercent = this.TaxPercent,
                Text = this.Text,
                PriceProfileId = this.PriceProfileId,
                PriceProfileName = this.PriceProfileName,
                ResourceId = this.ResourceId,
                Interval = this.Interval,
                SubOrder = this.SubOrder,
                SubOrderId = this.SubOrderId,
                Index = Index,
                IsCustomPricePcs = IsCustomPricePcs
            };
            return guestOrderLine;
        }
    }

    [Serializable]
    public class ArticleOrderLine : ResourceBasedOrderLine
    {
        public decimal? MeterStart { get; set; }
        public decimal? MeterEnd { get; set; }
        public Guid? MeterReference { get; set; }

        public override OrderLine Revert(Guid createAuthorId)
        {
            decimal number = 0 - this.Number;
            ArticleOrderLine articleOrderLine = new ArticleOrderLine()
            {
                CreateAuthorId = createAuthorId,
                OriginalCreateDate = this.OriginalCreateDate,
                TenantId = TenantId,
                Number = number,
                PricePcs = this.PricePcs,
                TaxPercent = this.TaxPercent,
                Tax = this.Tax,
                Text = this.Text,
                PriceProfileId = this.PriceProfileId,
                PriceProfileName = this.PriceProfileName,
                RevertedLineId = this.Id,
                ResourceId = this.ResourceId,
                SubOrder = this.SubOrder,
                SubOrderId = this.SubOrderId,
                Index = Index,
                IsCustomPricePcs = IsCustomPricePcs,
                MeterStart = MeterStart,
                MeterEnd = MeterEnd,
                MeterReference = MeterReference
            };
            return articleOrderLine;
        }

        public override OrderLine Release(Guid createAuthorId)
        {
            return null;
        }

        public override OrderLine Clone()
        {
            ArticleOrderLine articleOrderLine = new ArticleOrderLine()
            {
                Number = Number,
                OriginalCreateDate = this.OriginalCreateDate,
                TenantId = TenantId,
                PricePcs = this.PricePcs,
                Tax = this.Tax,
                TaxPercent = this.TaxPercent,
                Text = this.Text,
                PriceProfileId = this.PriceProfileId,
                PriceProfileName = this.PriceProfileName,
                ResourceId = this.ResourceId,
                SubOrder = this.SubOrder,
                SubOrderId = this.SubOrderId,
                Index = Index,
                IsCustomPricePcs = IsCustomPricePcs
            };
            return articleOrderLine;
        }
    }

    [Serializable]
    public class DiscountOrderLine : ResourceBasedOrderLine
    {
        public override OrderLine Revert(Guid createAuthorId)
        {
            decimal number = 0 - this.Number;
            DiscountOrderLine discountOrderLine = new DiscountOrderLine()
            {
                CreateAuthorId = createAuthorId,
                OriginalCreateDate = this.OriginalCreateDate,
                TenantId = TenantId,
                Number = number,
                PricePcs = this.PricePcs,
                Tax = this.Tax,
                TaxPercent = this.TaxPercent,
                Text = this.Text,
                PriceProfileId = this.PriceProfileId,
                PriceProfileName = this.PriceProfileName,
                RevertedLineId = this.Id,
                ResourceId = this.ResourceId,
                SubOrder = this.SubOrder,
                SubOrderId = this.SubOrderId,
                Index = Index
            };
            return discountOrderLine;
        }

        public override OrderLine Release(Guid createAuthorId)
        {
            return null;
        }

        public override OrderLine Clone()
        {
            DiscountOrderLine discountOrderLine = new DiscountOrderLine()
            {
                Number = Number,
                OriginalCreateDate = this.OriginalCreateDate,
                TenantId = TenantId,
                PricePcs = this.PricePcs,
                Tax = this.Tax,
                TaxPercent = this.TaxPercent,
                Text = this.Text,
                PriceProfileId = this.PriceProfileId,
                PriceProfileName = this.PriceProfileName,
                ResourceId = this.ResourceId,
                SubOrder = this.SubOrder,
                SubOrderId = this.SubOrderId,
                Index = Index
            };
            return discountOrderLine;
        }
    }

    [Serializable]
    public class SubOrderDiscountLine : ResourceBasedOrderLine
    {
        public Guid SubOrderDiscountId { get; set; }
        public SubOrderDiscount SubOrderDiscount { get; set; }
        public decimal DiscountPercent { get; set; }

        public override OrderLine Revert(Guid createAuthorId)
        {
            decimal number = 0 - this.Number;
            SubOrderDiscountLine discountOrderLine = new SubOrderDiscountLine()
            {
                CreateAuthorId = createAuthorId,
                OriginalCreateDate = this.OriginalCreateDate,
                TenantId = TenantId,
                Number = number,
                PricePcs = this.PricePcs,
                Tax = this.Tax,
                TaxPercent = this.TaxPercent,
                Text = this.Text,
                PriceProfileId = this.PriceProfileId,
                PriceProfileName = this.PriceProfileName,
                RevertedLineId = this.Id,
                ResourceId = this.ResourceId,
                SubOrder = this.SubOrder,
                SubOrderId = this.SubOrderId,
                Index = Index,
                SubOrderDiscountId = SubOrderDiscountId,
                DiscountPercent = DiscountPercent
            };
            return discountOrderLine;
        }

        public override OrderLine Release(Guid createAuthorId)
        {
            return null;
        }

        public override OrderLine Clone()
        {
            SubOrderDiscountLine discountOrderLine = new SubOrderDiscountLine()
            {
                Number = Number,
                OriginalCreateDate = this.OriginalCreateDate,
                TenantId = TenantId,
                PricePcs = this.PricePcs,
                Tax = this.Tax,
                TaxPercent = this.TaxPercent,
                Text = this.Text,
                PriceProfileId = this.PriceProfileId,
                PriceProfileName = this.PriceProfileName,
                ResourceId = this.ResourceId,
                SubOrder = this.SubOrder,
                SubOrderId = this.SubOrderId,
                Index = Index,
                SubOrderDiscountId = SubOrderDiscountId,
                DiscountPercent = DiscountPercent
            };
            return discountOrderLine;
        }
    }

}
