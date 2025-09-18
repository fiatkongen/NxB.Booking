using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;

namespace NxB.Dto.InventoryApi
{
    public class ProductOrderLineDto
    {
        public Guid ProductId { get; set; }
        public string Text { get; set; }
        public decimal Number { get; set; }
        public decimal Price { get; set; }
        public bool IsCustomPrice { get; set; }
        public Guid PriceProfileId { get; set; }
        public string PriceProfileName { get; set; }
        public decimal TaxPercent { get; set; }
    }
}
