using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.InventoryApi
{
    public class CreateProductDto
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Caption { get; set; }
        public string IconName { get; set; }

        public decimal FixedPrice { get; set; }
        public decimal? TaxPercent { get; set; }
        public bool IsDeleted { get; set; }
        public string BarCode { get; set; }
        public Dictionary<string, dynamic> Css { get; set; } = new();
    }

    public class ProductDto : CreateProductDto
    {
        public Guid Id { get; set; }
        public Guid PriceProfileId { get; set; }
        public string PriceProfileName { get; set; }
    }
}
