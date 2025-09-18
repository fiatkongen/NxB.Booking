using System;

namespace NxB.Dto.OrderingApi
{
    public class CreateDiscountDto
    {
        public string Name { get; set; }
        public decimal DiscountPercent { get; set; }
        public bool AllowMultiple { get; set; }
        public bool AllowPercentOverride { get; set; }
        public bool IsHidden { get; set; }

        public DiscountGroupSelectionDto RentalCategoriesSelection { get; set; }
        public DiscountGroupSelectionDto GuestCategoriesSelection { get; set; }
        public DiscountGroupSelectionDto ArticleCategoriesSelection { get; set; }
        public DiscountGroupSelectionDto CustomerGroupsSelection { get; set; }
    }

    public class DiscountDto : CreateDiscountDto
    {
        public Guid Id { get; set; }
    }
}
