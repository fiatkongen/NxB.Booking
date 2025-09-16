using Munk.AspNetCore;
using NxB.BookingApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Infrastructure
{
    public class ProductCategoryRepository : TenantFilteredRepository<ProductCategory, AppDbContext>, IProductCategoryRepository
    {
        protected override IQueryable<ProductCategory> TenantFilteredEntitiesQuery
        {
            get { return base.TenantFilteredEntitiesQuery.Include(x => x.ProductCategoryLinks); }
        }

        public ProductCategoryRepository(IClaimsProvider claimsProvider, AppDbContext appDbContext) : base(claimsProvider, appDbContext)
        {
        }

        public void Add(ProductCategory productCategory)
        {
            productCategory.ProductCategoryLinks.ForEach(x => x.ProductCategoryId = productCategory.Id);
            AppDbContext.Add(productCategory);
        }

        public void Add(IEnumerable<ProductCategory> articleCategories)
        {
            articleCategories.ToList().ForEach(Add);
        }

        public async Task Delete(Guid id)
        {
            var articleCategory = await FindSingle(id);
            AppDbContext.Remove(articleCategory);
        }

        public void Update(ProductCategory productCategory)
        {
            AppDbContext.Update(productCategory);
        }

        public async Task MarkAsDeleted(Guid id)
        {
            var articleCategory = await FindSingle(id);
            articleCategory.IsDeleted = true;
        }

        public async Task MarkAsUnDeleted(Guid id)
        {
            var articleCategory = await FindSingle(id);
            articleCategory.IsDeleted = false;
        }

        public async Task<ProductCategory> FindSingleOrDefault(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException(nameof(id));
            var articleCategory = TenantFilteredEntitiesQuery.SingleOrDefault(x => x.Id == id);
            return articleCategory;
        }

        private async Task<ProductCategory> FindSingle(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException(nameof(id));
            var articleCategory = TenantFilteredEntitiesQuery.Single(x => x.Id == id);
            return articleCategory;
        }

        public async Task<List<ProductCategory>> FindAll(bool includeDeleted = false)
        {
            return this.TenantFilteredEntitiesQuery.Where(x => includeDeleted || !x.IsDeleted).OrderBy(x => x.Sort).ToList();
        }
    }
}
