using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NxB.Domain.Common.Interfaces;
using NxB.BookingApi.Exceptions;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class SubOrderArticle : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set;  }
        public Order Order { get; set; }
        public Guid OrderId { get; set; }
        public int Index { get; set; }
        public string Note { get; set; }
        public bool NoteState { get; set; }
        public bool IsEqualized { get; private set; }

        public List<OrderLine> OrderLines { get; } = new();

        public List<SubOrderSection> SubOrderSections { get; set; } = new();
        public List<SubOrderDiscount> SubOrderDiscounts { get; set; } = new();

        public List<ArticleOrderLine> ArticleOrderLines
        {
            get => OrderLines.OfType<ArticleOrderLine>().ToList();
            private set => OrderLines.AddRange(value);  //used by mapper
        }

        public List<ArticleOrderLine> NotPersistedArticleOrderLines
        {
            get { return this.ArticleOrderLines.Where(x => x.Id == Guid.Empty).ToList(); }
        }

        public List<DiscountOrderLine> DiscountOrderLines
        {
            get => OrderLines.OfType<DiscountOrderLine>().ToList();
            private set => OrderLines.AddRange(value);  //used by mapper
        }

        public List<SubOrderDiscountLine> NotPersistedSubOrderDiscountLines
        {
            get { return this.SubOrderDiscountLines.Where(x => x.Id == Guid.Empty).ToList(); }
        }

        public List<SubOrderDiscountLine> SubOrderDiscountLines
        {
            get => OrderLines.OfType<SubOrderDiscountLine>().ToList();
            private set => OrderLines.AddRange(value);  //used by mapper
        }

        public List<DiscountOrderLine> NotPersistedDiscountOrderLines
        {
            get { return this.DiscountOrderLines.Where(x => x.Id == Guid.Empty).ToList(); }
        }

        public virtual void EqualizeOrderLines()
        {
            this.IsEqualized = OrderLines.TrueForAll(x => x.IsEqualized);

            if (this.IsEqualized && OrderLines.Sum(x => x.PricePcs * x.Number) != 0)
            {
                throw new AddSubOrdersException("Alle linier er udlignede, men sum er ikke 0", null);
            }
        }

        public virtual SubOrderSection AugmentSubOrderSectionsAndOrderLines(Guid createAuthorId, Guid tenantId)
        {
            SubOrderSection subOrderSection = new SubOrderSection
            {
                Id = Guid.NewGuid(),
                SubOrder = this,
                SubOrderId = this.Id,
                CreateAuthorId = createAuthorId
            };
            SubOrderSections.Add(subOrderSection);

            NotPersistedArticleOrderLines.ForEach(line =>
            {
                line.Id = Guid.NewGuid();
                line.TenantId = TenantId;
                line.CreateAuthorId = createAuthorId;
                line.OriginalCreateDate ??= line.CreateDate;
                line.SubOrderId = Id;
                line.SubOrder = this;
                line.SubOrderSectionId = subOrderSection.Id;
                line.SubOrderSection = subOrderSection;
                subOrderSection.OrderLines.Add(line);
            });
            NotPersistedDiscountOrderLines.ForEach(line =>
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
            NotPersistedSubOrderDiscountLines.ForEach(line =>
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
    }
}
