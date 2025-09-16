using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Itenso.TimePeriod;
using Munk.AspNetCore;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class ProductCategory : ITenantEntity, ICreateAudit
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Guid CreateAuthorId { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now.ToEuTimeZone();
        public int Sort { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Caption { get; set; }
        public string IconName { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public string CssJson { get; set; }
        public bool HasCustomBackgroundColor { get; set; }

        public List<ProductCategoryLink> ProductCategoryLinks { get; set; } = new();

        public ProductCategory(Guid id, Guid tenantId, Guid createAuthorId)
        {
            Id = id;
            TenantId = tenantId;
            CreateAuthorId = createAuthorId;
        }

        public void AddCategory(ProductCategoryLink productCategoryLink)
        {
            productCategoryLink.ProductCategoryId = Id;
            ProductCategoryLinks.Add(productCategoryLink);
        }
    }
}
