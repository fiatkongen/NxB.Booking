using System;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class SubOrderDiscount
    {
        public Guid Id { get; private set; }

        public Guid DiscountId { get; private set; }
        public Discount Discount { get; private set; }

        public Guid SubOrderId { get; private set; }
        public SubOrder SubOrder { get; private set; }

        public decimal DiscountPercent { get; set; }
        public int Index { get; set; }

        public string Text { get; set; }
        public bool IsDeleted { get; set; }

        private SubOrderDiscount()
        {}

        public SubOrderDiscount(Guid id, Guid discountId, Guid subOrderId, decimal discountPercent, int index)
        {
            Id = id;
            DiscountId = discountId;
            SubOrderId = subOrderId;
            DiscountPercent = discountPercent;
            Index = index;
        }

        public void MarkAsDeleted()
        {
            IsDeleted = true;
        }
    }
}