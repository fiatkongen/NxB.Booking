using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using NxB.Dto.AllocationApi;
using NxB.Clients.Interfaces;
using NxB.Dto.InventoryApi;

namespace NxB.Clients
{
    public class ArticleClient : NxBAdministratorClientWithTenantUrlLookup, IArticleClient
    {
        public ArticleClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public virtual async Task<List<ArticleDto>> FindAll(bool includeDeleted)
        {
            var url = $"/NxB.Services.App/NxB.InventoryApi/article/list/all?includeDeleted=" + includeDeleted;
            var articles = await this.GetAsync<List<ArticleDto>>(url);
            return articles;
        }

        public async Task<List<ArticleDto>> FindAllOnlineFromTenantId(Guid tenantId)
        {
            var url = $"/NxB.Services.App/NxB.InventoryApi/article/list/all/online/tenant?tenantId=" + tenantId;
            var articles = await this.GetAsync<List<ArticleDto>>(url);
            return articles;
        }

        public virtual async Task<ArticleDto> FindSingleOrDefault(Guid id)
        {
            var url = $"/NxB.Services.App/NxB.InventoryApi/article?id=" + id;
            var article = await this.GetAsync<ArticleDto>(url);
            return article;
        }


        public async Task<ArticleDto> FindSingleOrDefaultFromLegacyId(long legacyId)
        {
            var url = $"/NxB.Services.App/NxB.InventoryApi/article/legacyId?legacyId=" + legacyId;
            var articleDto = await this.GetAsync<ArticleDto>(url);
            return articleDto;
        }

        public async Task<ArticleDto> FindMiscArticle()
        {
            var url = $"/NxB.Services.App/NxB.InventoryApi/article/misc";
            var article = await this.GetAsync<ArticleDto>(url);
            return article;
        }
    }

    public class ArticleClientCached : ArticleClient, IArticleClientCached
    {
        private readonly List<ArticleDto> _cache = new();

        public ArticleClientCached(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public override async Task<List<ArticleDto>> FindAll(bool includeDeleted)
        {
            var articleDtos = await base.FindAll(includeDeleted);
            _cache.AddRange(articleDtos);
            return articleDtos;
        }

        public override async Task<ArticleDto> FindSingleOrDefault(Guid id)
        {
            var item = _cache.SingleOrDefault(x => x.Id == id);
            if (item == null)
            {
                item = await base.FindSingleOrDefault(id);
                if (item != null)
                {
                    _cache.Add(item);
                }
            }
            return item;
        }

    }
}

