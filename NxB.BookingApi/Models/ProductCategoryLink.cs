using NxB.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class ProductCategoryLink
    {
        public Guid ProductId { get; set; }
        public Guid ProductCategoryId { get; set; }
        public int Sort { get; set; }
    }
}
