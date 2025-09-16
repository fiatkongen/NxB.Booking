using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Munk.Utils.Object;
using NxB.Domain.Common.Dto;
using NxB.Dto.AllocationApi;

namespace NxB.Dto.InventoryApi
{
    public class BaseArticleDto
    {
        public Dictionary<string, string> NameTranslations { get; set; }
        public Dictionary<string, string> DescriptionTranslations { get; set; }
        public int Sort { get; set; }

        public List<ArticleRentalCategoryDto> ArticleRentalCategories { get; set; }
        public bool AllowCustomText { get; set; }
        public bool IsAvailableOnline { get; set; }
        public BookingAvailability KioskAvailability { get; set; }
        public decimal? Tax { get; set; }
        public bool IsAvailablePOS { get; set; }
        public bool IsAvailableExtras { get; set; } = true;
        public string BarCode { get; set; }
        public bool ExcemptFromOnlineSearchTotal { get; set; }
    }

    public class CreateArticleDto : BaseArticleDto
    {
        public decimal? FixedPrice { get; set; }
    }

    public class ArticleDto : BaseArticleDto, ICategoryDto
    {
        [NoEmpty]
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class ModifyArticleDto : CreateArticleDto
    {
        [Required]
        [NoEmpty]
        public Guid Id { get; set; }
    }
}
