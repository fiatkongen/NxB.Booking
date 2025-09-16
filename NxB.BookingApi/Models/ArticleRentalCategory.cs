using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.BookingApi.Models
{
    public class ArticleRentalCategory
    {
        public Guid RentalCategoryId { get; set; }
        public int? MaxArticles { get; set; }
        public int? MinArticles { get; set; }
        public int? DefaultArticles { get; set; }

        public ArticleRentalCategory(Guid rentalCategoryId)
        {
            RentalCategoryId = rentalCategoryId;
        }
    }
}
