using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using NxB.Domain.Common.Interfaces;
using NxB.Clients.Interfaces;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class ArticleRepository : TenantFilteredRepository<Article, AppDbContext>, IArticleRepository
    {
        private readonly IPriceProfileClient _priceProfileClient;

        protected override IQueryable<Article> TenantFilteredEntitiesQuery => base.TenantFilteredEntitiesQuery.OrderBy(x => x.Sort);

        public ArticleRepository(IClaimsProvider claimsProvider, AppDbContext appDbContext, IPriceProfileClient priceProfileClient) : base(claimsProvider, appDbContext)
        {
            _priceProfileClient = priceProfileClient;
        }

        public void Add(Article article)
        {
            AppDbContext.Add(article);
        }

        public void Add(IEnumerable<Article> articles)
        {
            articles.ToList().ForEach(Add);
        }

        public async Task Delete(Guid id)
        {
            var article = await FindSingle(id);
            this.AppDbContext.Articles.Remove(article);
            await _priceProfileClient.DeleteForResource(article.Id);
        }

        public void Update(Article article)
        {
            AppDbContext.Update(article);
        }

        public async Task MarkAsDeleted(Guid id)
        {
            var article = await FindSingle(id);
            article.IsDeleted = true;
        }

        public async Task MarkAsUnDeleted(Guid id)
        {
            var article = await FindSingle(id);
            article.IsDeleted = false;
        }

        public Task<Article> FindSingleOrDefault(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException(nameof(id));
            var article = AppDbContext.Articles.SingleOrDefaultAsync(x => x.Id == id);
            return article;
        }

        public async Task<Article> FindMiscArticle()
        {
            var articles = await this.TenantFilteredEntitiesQuery.Where(x => !x.IsDeleted && x.IsAvailableExtras && x.NameTranslationsJson.Contains("Diverse")).ToListAsync();
            foreach (var article in articles)
            {
                if (article.NameTranslations.ContainsKey("s_da") && article.NameTranslations["s_da"] == "Diverse")
                    return article;
            }

            return null;
        }

        public Task<Article> FindSingle(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException(nameof(id));
            var article = AppDbContext.Articles.SingleAsync(x => x.Id == id);
            return article;
        }

        public async Task<List<Article>> FindAll(bool includeDeleted)
        {
            var articles = await this.TenantFilteredEntitiesQuery.Where(x => (!x.IsDeleted || includeDeleted) && x.IsAvailableExtras).OrderBy(x => x.Sort).ToListAsync();
            return articles;
        }

        public async Task<List<Article>> FindAllProducts(bool includeDeleted)
        {
            var articles = await this.TenantFilteredEntitiesQuery.Where(x => (!x.IsDeleted || includeDeleted) && x.IsAvailablePOS).OrderBy(x => x.Sort).ToListAsync();
            return articles;
        }

        public async Task<List<Article>> FindAllOnlineFromTenantId(Guid tenantId)
        {
            var articles = await this.AppDbContext.Articles.Where(x => x.IsAvailableOnline && !x.IsDeleted && x.TenantId == tenantId && x.IsAvailableExtras).OrderBy(x => x.Sort).ToListAsync();
            return articles;
        }

        public async Task<List<Article>> FindAllFromTenantId(Guid tenantId)
        {
            var articles = await this.AppDbContext.Articles.Where(x => !x.IsDeleted && x.TenantId == tenantId && x.IsAvailableExtras).OrderBy(x => x.Sort).ToListAsync();
            return articles;
        }

        public async  Task<List<Article>> FindAllIncludeDeleted()
        {
            var articles = await this.TenantFilteredEntitiesQuery.Where(x => x.IsAvailableExtras).OrderBy(x => x.Sort).ToListAsync();
            return articles;
        }

        public async Task<List<Article>> FindAllOnline()
        {
            var articles = await this.TenantFilteredEntitiesQuery.Where(x => !x.IsDeleted && x.IsAvailableOnline && x.IsAvailableExtras).OrderBy(x => x.Sort).ToListAsync();
            return articles;
        }
    }
}
