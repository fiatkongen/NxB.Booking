using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace NxB.BookingApi.Models
{
    public interface IArticleRepository
    {
        void Add(Article article);
        void Add(IEnumerable<Article> articles);
        Task Delete(Guid id);
        void Update(Article article);
        Task MarkAsDeleted(Guid id);
        Task MarkAsUnDeleted(Guid id);
        Task<Article> FindSingleOrDefault(Guid id);
        Task<Article> FindMiscArticle();
        Task<Article> FindSingle(Guid id);
        Task<List<Article>> FindAll(bool includeDeleted);
        Task<List<Article>> FindAllProducts(bool includeDeleted);
        Task<List<Article>> FindAllOnlineFromTenantId(Guid tenantId);
        Task<List<Article>> FindAllFromTenantId(Guid tenantId);
        Task<List<Article>> FindAllOnline();
    }
}
