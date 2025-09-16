using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using Munk.Utils.Object;
using NxB.Domain.Common.Dto;
using NxB.Dto.AllocationApi;
using NxB.Dto.Clients;
using NxB.Dto.InventoryApi;
using NxB.Dto.PricingApi;
using NxB.Dto.TenantApi;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Controllers.Inventory
{
    [Produces("application/json")]
    [Route("article")]
    [Authorize]
    [ApiValidationFilter]
    public class ArticleController : BaseController
    {
        private readonly IArticleRepository _articleRepository;
        private readonly ProductFactory _productFactory;
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly ITenantClient _tenantClient;

        public ArticleController(IArticleRepository articleRepository, ProductFactory productFactory, AppDbContext appDbContext, IMapper mapper, ITenantClient tenantClient)
        {
            _articleRepository = articleRepository;
            _productFactory = productFactory;
            _appDbContext = appDbContext;
            _mapper = mapper;
            _tenantClient = tenantClient;
        }

        [HttpGet]
        [Route("")]
        public async Task<ObjectResult> FindSingleArticle(Guid id)
        {
            var article = await _articleRepository.FindSingleOrDefault(id);
            if (article == null)
            {
                return new ObjectResult(null);
            }
            var articleDto = _mapper.Map<ArticleDto>(article);
            return new OkObjectResult(articleDto);
        }

        [HttpGet]
        [Route("misc")]
        public async Task<ObjectResult> FindMiscArticle()
        {
            var article = await _articleRepository.FindMiscArticle();
            if (article == null)
            {
                return new ObjectResult(null);
            }
            var articleDto = _mapper.Map<ArticleDto>(article);
            return new OkObjectResult(articleDto);
        }

        [HttpPost]
        [Route("")]
        public async Task<ObjectResult> CreateArticle([FromBody] CreateArticleDto createArticleDto)
        {
            var article = await _productFactory.Create(createArticleDto.FixedPrice);
            _mapper.Map(createArticleDto, article);
            _articleRepository.Add(article);
            
            await _appDbContext.SaveChangesAsync();

            article = await _articleRepository.FindSingle(article.Id);
            var articleDto = _mapper.Map<ArticleDto>(article);

            return new CreatedResult(new Uri("?id=" + articleDto.Id, UriKind.Relative), articleDto);
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAllArticles(bool includeDeleted)
        {
            var articles = (await _articleRepository.FindAll(includeDeleted)).ToList();
            var articleDtos = articles.Select(x => _mapper.Map<ArticleDto>(x));
            return new OkObjectResult(articleDtos);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("list/all/online/tenant")]
        public async Task<ObjectResult> FindAllOnlineArticlesFromTenantId(Guid tenantId)
        {
            var articles = (await _articleRepository.FindAllOnlineFromTenantId(tenantId)).ToList();
            var articleDtos = articles.Select(x => _mapper.Map<ArticleDto>(x));
            return new OkObjectResult(articleDtos);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("list/all/tenant")]
        public async Task<ObjectResult> FindAllArticlesTenant(Guid tenantId)
        {
            await SignInFakeOnlineUserForTenant(this._tenantClient, tenantId);
            var articles = (await _articleRepository.FindAllFromTenantId(tenantId)).ToList();
            var articleDtos = articles.Select(x => _mapper.Map<ArticleDto>(x));
            return new OkObjectResult(articleDtos);
        }

        [HttpPut]
        [Route("")]
        public async Task<ObjectResult> ModifyArticle([FromBody] ModifyArticleDto modifyArticleDto)
        {
            var article = await _articleRepository.FindSingle(modifyArticleDto.Id);
            _mapper.Map(modifyArticleDto, article);
            _articleRepository.Update(article);
            await _appDbContext.SaveChangesAsync();

            var articleDto = _mapper.Map<ArticleDto>(article);

            return new OkObjectResult(articleDto);
        }

        [HttpDelete]
        [Route("")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteArticlePermanently([NoEmpty]Guid id)
        {
            await _articleRepository.Delete(id);
            await _appDbContext.SaveChangesAsync();

            return new OkResult();
        }

        [HttpPut]
        [Authorize]
        [Route("markdeleted")]
        public async Task<ObjectResult> MarkArticleAsDeleted([NoEmpty]Guid id, bool isDeleted = true)
        {
            var article = await _articleRepository.FindSingle(id);
            if (article == null) return new NotFoundObjectResult(null);

            if (isDeleted)
            {
                await _articleRepository.MarkAsDeleted(id);
            }
            else
            {
                await _articleRepository.MarkAsUnDeleted(id);
            }
            await _appDbContext.SaveChangesAsync();

            article = await _articleRepository.FindSingle(id);
            var articleDto = _mapper.Map<ArticleDto>(article);

            return new OkObjectResult(articleDto);
        }
    }
}
