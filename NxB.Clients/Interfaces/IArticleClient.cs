using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NxB.Dto.InventoryApi;

namespace NxB.Clients.Interfaces
{
    public interface IArticleClient : IAuthorizeClient
    {
        Task<List<ArticleDto>> FindAll(bool includeDeleted);
        Task<List<ArticleDto>> FindAllOnlineFromTenantId(Guid tenantId);
        Task<ArticleDto> FindSingleOrDefault(Guid id);
        Task<ArticleDto> FindSingleOrDefaultFromLegacyId(long legacyId);
        Task<ArticleDto> FindMiscArticle();
    }

    public interface IArticleClientCached : IArticleClient { }
}