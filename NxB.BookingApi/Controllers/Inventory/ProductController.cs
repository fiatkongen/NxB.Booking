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
using NxB.Clients.Interfaces;
using NxB.Dto.InventoryApi;
using NxB.Dto.PricingApi;
using NxB.Dto.TenantApi;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Controllers.Inventory
{
    [Produces("application/json")]
    [Route("product")]
    [Authorize]
    [ApiValidationFilter]
    public class ProductController : BaseController
    {
        private readonly IArticleRepository _articleRepository;
        private readonly ProductFactory _productFactory;
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IPriceProfileClient _priceProfileClient;

        public ProductController(IArticleRepository articleRepository, ProductFactory productFactory, AppDbContext appDbContext, IMapper mapper, IPriceProfileClient priceProfileClient)
        {
            _articleRepository = articleRepository;
            _productFactory = productFactory;
            _appDbContext = appDbContext;
            _mapper = mapper;
            _priceProfileClient = priceProfileClient;
        }

        [HttpGet]
        [Route("")]
        public async Task<ObjectResult> FindSingleProduct(Guid id)
        {
            var article = await _articleRepository.FindSingleOrDefault(id);
            if (article == null)
            {
                return null;
            }
            var productDto = _mapper.Map<ProductDto>(article);
            return new OkObjectResult(productDto);
        }

        [HttpPost]
        [Route("")]
        public async Task<ObjectResult> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            var article = await _productFactory.CreateProduct(createProductDto.FixedPrice);
            _mapper.Map(createProductDto, article);
            _articleRepository.Add(article);
            
            await _appDbContext.SaveChangesAsync();

            article = await _articleRepository.FindSingle(article.Id);
            var productDto = await MapProductDto(article);
            return new CreatedResult(new Uri("?id=" + productDto.Id, UriKind.Relative), productDto);
        }

        private async Task<ProductDto> MapProductDto(Article article)
        {
            var productDto = _mapper.Map<ProductDto>(article);
            var priceProfile = await _priceProfileClient.FindSingleOrDefaultFromResourceId(article.Id);
            productDto.FixedPrice = priceProfile.FixedPrice.Value;
            productDto.PriceProfileId = priceProfile.Id;
            productDto.PriceProfileName = priceProfile.Name;
            productDto.TaxPercent = article.Tax;
            return productDto;
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAllProducts(bool includeDeleted = false)
        {
            var articles = (await _articleRepository.FindAllProducts(includeDeleted)).ToList();
//            var productDtos = articles.Select(x => _mapper.Map<ProductDto>(x));
            var productDtos = new List<ProductDto>();

            foreach (var article in articles)
            {
                var productDto = await MapProductDto(article);
                productDtos.Add(productDto);
            }

            return new OkObjectResult(productDtos);
        }


        [HttpPut]
        [Route("")]
        public async Task<ObjectResult> ModifyArticle([FromBody] ProductDto modifyProductDto)
        {
            var article = await _articleRepository.FindSingle(modifyProductDto.Id);
            _mapper.Map(modifyProductDto, article);
            var priceProfile = await _priceProfileClient.FindSingleOrDefaultFromResourceId(article.Id);
            await _priceProfileClient.ModifyFixedPrice(priceProfile.Id, modifyProductDto.FixedPrice);

            _articleRepository.Update(article);
            await _appDbContext.SaveChangesAsync();

            var productDto = await MapProductDto(article);
            return new OkObjectResult(productDto);
        }

        [HttpDelete]
        [Route("")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProductPermanently([NoEmpty] Guid id)
        {
            await _articleRepository.Delete(id);
            await _appDbContext.SaveChangesAsync();

            return new OkResult();
        }

        [HttpPut]
        [Authorize]
        [Route("markdeleted")]
        public async Task<IActionResult> MarkArticleAsDeleted([NoEmpty]Guid id, bool isDeleted = true)
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

            return new OkResult();
        }
    }
}
