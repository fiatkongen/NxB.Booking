using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public interface IProductCategoryRepository
    {
        void Add(ProductCategory productCategory);
        void Add(IEnumerable<ProductCategory> articleCategories);
        Task Delete(Guid id);
        void Update(ProductCategory product);
        Task MarkAsDeleted(Guid id);
        Task MarkAsUnDeleted(Guid id);
        Task<ProductCategory> FindSingleOrDefault(Guid id);
        Task<List<ProductCategory>> FindAll(bool includeDeleted = false);
    }
}
