using NxB.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.InventoryApi
{
    public class ProductCategoryLinkDto
    {
        public Guid ProductId { get; set; }
        public int Sort { get; set; }
        public ProductDto Product { get; set; }
    }
}
