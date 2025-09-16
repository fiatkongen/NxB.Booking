using NxB.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.InventoryApi
{
    public class CreateProductCategoryDto
    {
        public int Sort { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Caption { get; set; }
        public string IconName { get; set; }
        public string Description { get; set; }
        public bool HasCustomBackgroundColor { get; set; }

        public List<ProductCategoryLinkDto> ProductCategoryLinks { get; set; } = new();
        public Dictionary<string, dynamic> Css { get; set; } = new();
    }

    public class ProductCategoryDto : CreateProductCategoryDto
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class ModifyProductCategoryDto : ProductCategoryDto
    {
    }
}
