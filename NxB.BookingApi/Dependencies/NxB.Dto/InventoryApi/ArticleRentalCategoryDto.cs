using System;

namespace NxB.Dto.InventoryApi
{
    public class ArticleRentalCategoryDto
    {
        public Guid RentalCategoryId { get; set; }
        public int? MaxArticles { get; set; }
        public int? MinArticles { get; set; }
        public int? DefaultArticles { get; set; }
    }
}
