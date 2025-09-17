using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Itenso.TimePeriod;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using Munk.Utils.Object;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;
using NxB.Dto.OrderingApi;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Extensions;
using ServiceStack;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class Order : ITenantEntity, IOrderKey
    {
        public Guid Id { get; internal set; }
        public DateTime? CreateDate { get; set; } = DateTime.Now.ToEuTimeZone();
        public DateTime? LastEditDate { get; internal set; }
        public decimal? LastSumTotal { get; internal set; }
        public Guid TenantId { get; internal set; }
        public long FriendlyId { get; internal set; }
        public Guid AccountId { get; internal set; }
        public bool IsDeleted { get; private set; }
        public DateTime? ImportTimeStamp { get; set; }
        public string Note { get; set; }
        public bool NoteState { get; set; }
        public CreatedBy CreatedBy { get; set; }
        public string CreatedByExternalId { get; set; }
        public string CreateNote { get; set; }
        public string OnlineTransactionDetails { get; set; }
        public string ExternalId { get; set; }

        public DateInterval DateInterval => SubOrders.Where(x => x.DateInterval != null).DateInterval();
        public List<SubOrder> SubOrders { get; set; } = new();
        public List<SubOrder> SubOrdersNotEqualized() => this.SubOrders.Where(x => !x.IsEqualized).ToList();
        public IList<Allocation> Allocations => SubOrders.SelectMany(x => x.Allocations).ToList();

        private Order() { }

        public Order(Guid id)
        {
            Id = id;
        }

        public Order(Guid id, Guid tenantId, int friendlyId, Guid accountId)
        {
            Id = id;
            TenantId = tenantId;
            FriendlyId = friendlyId;
            AccountId = accountId;
        }

        private void AddIfNotAdded(IEnumerable<SubOrder> subOrders)
        {
            subOrders.ToList().ForEach(AddIfNotAdded);
        }

        private void AddIfNotAdded(SubOrder subOrder)
        {
            if (!SubOrders.Exists(x => x.Id == subOrder.Id))
            {
                SubOrders.Add(subOrder);
            }

        }

        private void CorrectIndexes()
        {
            var unIndexedSubOrders = SubOrders.Where(x => x.Index <= 0).ToList();
            int maxSubOrderIndex = SubOrders.Max(x => x.Index);
            maxSubOrderIndex = maxSubOrderIndex < 0 ? 0 : maxSubOrderIndex;
            unIndexedSubOrders.ForEach(x => x.Index = ++maxSubOrderIndex);

            var subOrderDiscounts = SubOrders.SelectMany(x => x.SubOrderDiscounts).ToList();
            var unIndexedSubOrderDiscounts = subOrderDiscounts.Where(x => x.Index <= 0).OrderByDescending(x => x.Index).ToList();
            if (unIndexedSubOrderDiscounts.Count > 0)
            {
                int maxSubOrderDiscountsIndex = subOrderDiscounts.Count > 0 ? subOrderDiscounts.Max(x => x.Index) : 0;
                maxSubOrderDiscountsIndex = maxSubOrderDiscountsIndex < 0 ? 0 : maxSubOrderDiscountsIndex;
                unIndexedSubOrderDiscounts.ForEach(x => x.Index = ++maxSubOrderDiscountsIndex);
            }

            var orderLines = SubOrders.SelectMany(x => x.OrderLines).ToList();
            var unIndexedOrderLines = orderLines.Where(x => x.Index <= 0).OrderByDescending(x => x.Index).ToList();
            decimal maxOrderLinesIndex = Math.Floor(orderLines.Max(x => x.Index));
            maxOrderLinesIndex = maxOrderLinesIndex < 0 ? 0 : maxOrderLinesIndex;

            decimal childIndex = 0.000m;
            decimal grandChildIndex = 0.000000m;
            unIndexedOrderLines.ForEach(x =>
            {

                if (x.Index % 1 == 0)   //First level
                {
                    x.Index = ++maxOrderLinesIndex;
                    childIndex = 0;
                    grandChildIndex = 0;
                }
                else
                {
                    if ((x.Index * 1000) % 1 == 0)   //second level
                    {
                        childIndex += 0.001m;
                        x.Index = maxOrderLinesIndex + childIndex;
                        grandChildIndex = 0;
                    }
                    else //third level
                    {
                        grandChildIndex += 0.000001m;
                        x.Index = maxOrderLinesIndex + childIndex + grandChildIndex;
                    }
                }
            });

            var subOrderSections = SubOrders.SelectMany(x => x.SubOrderSections).ToList();
            var unIndexSubOrderSections = subOrderSections.Where(x => x.Index == 0).ToList();
            int maxSubOrderSectionIndex = subOrderSections.Max(x => x.Index);
            maxSubOrderSectionIndex = maxSubOrderSectionIndex < 0 ? 0 : maxSubOrderSectionIndex;
            unIndexSubOrderSections.ForEach(x => x.Index = ++maxSubOrderSectionIndex);
        }

        /// <summary>
        /// Adds an already existing suborder which has been moved from another booking
        /// </summary>
        /// <param name="subOrder"></param>
        public void AddExistingSubOrder(SubOrder subOrder)
        {
            if (subOrder.Order != null) throw new ArgumentException(nameof(subOrder.Order));
            if (subOrder.OrderId != Guid.Empty) throw new ArgumentException(nameof(subOrder.Order));

            subOrder.Order = this;
            subOrder.OrderId = this.Id;
            this.AddIfNotAdded(subOrder);
            subOrder.Index = -1;
            this.CorrectIndexes();
            UpdateLastValues();
        }

        public void AddSubOrderDiscount(SubOrder subOrder, SubOrderDiscount subOrderDiscount)
        {
            subOrder.SubOrderDiscounts.Add(subOrderDiscount);
            CorrectIndexes();
            UpdateLastValues();
        }

        public void MarkSubOrderDiscountsAsDeleted(List<SubOrder> subOrders, List<Guid> subOrderDiscountIds)
        {
            subOrders.ForEach(x => x.MarkSubOrderDiscountsAsDeleted(subOrderDiscountIds));
        }

        public (List<SubOrder> created, List<SubOrder> modified) AddOrAppendToSubOrders(List<SubOrder> subOrders, Guid createAuthorId)
        {
            var newSubOrders = subOrders.Where(s => s.Id == Guid.Empty).ToList();
            var modifiedSubOrders = subOrders.Where(s => s.Id != Guid.Empty).ToList();

            AugmentNewlyCreatedSubOrders(newSubOrders, createAuthorId);
            AugmentModifiedSubOrders(modifiedSubOrders, createAuthorId);
            AddIfNotAdded(newSubOrders);
            CorrectIndexes();
            UpdateLastValues();
            return (newSubOrders, modifiedSubOrders);
        }

        private void UpdateLastValues()
        {
            try
            {
                this.LastEditDate = DateTime.Now.ToEuTimeZone();
                var orderLines = this.SubOrders.SelectMany(x => x.OrderLines).Where(x => !x.IsEqualized).ToList();
                this.LastSumTotal = orderLines.Sum(x => x.Total);
            }
            catch { }
        }

        private void AugmentNewlyCreatedSubOrders(List<SubOrder> subOrders, Guid createAuthorId)
        {
            subOrders.Where(s => s.Id == Guid.Empty).ToList().ForEach(subOrder =>
            {
                subOrder.Id = Guid.NewGuid();
                subOrder.TenantId = TenantId;
                subOrder.OrderId = Id;
                subOrder.Order = this;
                subOrder.AugmentSubOrderSectionsAndOrderLines(createAuthorId, TenantId);
                subOrder.UpdateDateInterval();
            });
        }

        private void AugmentModifiedSubOrders(List<SubOrder> subOrders, Guid createAuthorId)
        {
            subOrders.Where(s => s.Id != Guid.Empty).ToList().ForEach(subOrder =>
            {
                var existingSubOrder = SubOrders.Single(x => x.Id == subOrder.Id);
                existingSubOrder.OrderLines.AddRange(subOrder.OrderLines);
                existingSubOrder.SubOrderDiscounts.AddRange(subOrder.SubOrderDiscounts);
                existingSubOrder.AugmentSubOrderSectionsAndOrderLines(createAuthorId, TenantId);
                existingSubOrder.EqualizeOrderLines();
            });
        }

        public void RevertSubOrder(Guid subOrderId, Guid createAuthorId)
        {
            var subOrder = this.SubOrders.First(x => x.Id == subOrderId);
            subOrder.Revert(createAuthorId, TenantId);
            CorrectIndexes();
            UpdateLastValues();
        }

        public void ReleaseSubOrder(Guid subOrderId, Guid createAuthorId)
        {
            var subOrder = this.SubOrders.First(x => x.Id == subOrderId);
            subOrder.Release(createAuthorId, TenantId);
            subOrder.UpdateDateInterval();
            CorrectIndexes();
            UpdateLastValues();
        }

        public decimal CalculateTotal()
        {
            return this.SubOrders.SelectMany(x => x.OrderLines.Where(ol => !ol.IsEqualized))
                .Sum(ol => ol.PricePcs * ol.Number);
        }

        public SubOrder RemoveSubOrder(Guid subOrderId)
        {
            try
            {
                var subOrder = SubOrders.Single(x => x.Id == subOrderId);
                subOrder.Order = null;
                subOrder.OrderId = Guid.Empty;
                this.SubOrders.Remove(subOrder);
                UpdateLastValues();
                return subOrder;
            }
            catch (Exception exception)
            {
                throw new MoveSubOrderException(subOrderId, this, exception);
            }
        }
    }
}
