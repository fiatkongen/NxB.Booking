using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Munk.Utils.Object;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;
using NxB.Dto.InventoryApi;
using NxB.Dto.PricingApi;
using NxB.Clients.Interfaces;

namespace NxB.BookingApi.Controllers.Inventory
{
    [Produces("application/json")]
    [Route("productcategory")]
    [Authorize]
    [ApiValidationFilter]
    public class ProductCategoryController : BaseController
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly ProductCategoryFactory _productCategoryFactory;
        private readonly IArticleRepository _articleRepository;
        private readonly IPriceProfileClient _priceProfileClient;

        public ProductCategoryController(AppDbContext appDbContext, IMapper mapper, IProductCategoryRepository productCategoryRepository, ProductCategoryFactory productCategoryFactory, IArticleRepository articleRepository, IPriceProfileClient priceProfileClient)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _productCategoryRepository = productCategoryRepository;
            _productCategoryFactory = productCategoryFactory;
            _articleRepository = articleRepository;
            _priceProfileClient = priceProfileClient;
        }

        [HttpGet]
        [Route("")]
        public async Task<ObjectResult> FindSingleProductCategory(Guid id)
        {
            var productCategory = await _productCategoryRepository.FindSingleOrDefault(id);
            if (productCategory == null)
            {
                return null;
            }
            var productCategoryDto = _mapper.Map<ProductCategoryDto>(productCategory);

            var products = await _articleRepository.FindAllProducts(true);
            var productIds = products.Select(x => x.Id).ToList();


            productCategoryDto.ProductCategoryLinks = productCategoryDto.ProductCategoryLinks.Where(cl => productIds.Contains(cl.ProductId)).ToList();

            productCategoryDto.ProductCategoryLinks.ForEach(x =>
                x.Product = _mapper.Map<ProductDto>(products.Single(p => p.Id == x.ProductId)));

            return new OkObjectResult(productCategoryDto);
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAllProductCategories(bool includeDeleted)
        {
            var productCategories = await _productCategoryRepository.FindAll(includeDeleted);
            var productCategoryDtos = productCategories.Select(x => _mapper.Map<ProductCategoryDto>(x)).ToList();

            var products = await _articleRepository.FindAllProducts(includeDeleted);
            var productIds = products.Select(x => x.Id).ToList();

            productCategoryDtos.ForEach(x => 
                x.ProductCategoryLinks = x.ProductCategoryLinks.Where(cl => productIds.Contains(cl.ProductId)).ToList());

            var productCategoryLinkDtos = productCategoryDtos.SelectMany(x => x.ProductCategoryLinks).ToList();
            foreach (var x in productCategoryLinkDtos)
            {
                x.Product = await MapProductDto(products.Single(p => p.Id == x.ProductId));
            }

            return new OkObjectResult(productCategoryDtos);
        }

        private async Task<ProductDto> MapProductDto(Article article)
        {
            var productDto = _mapper.Map<ProductDto>(article);
            var priceProfile = await _priceProfileClient.FindSingleOrDefaultFromResourceId(article.Id);
            productDto.FixedPrice = priceProfile.FixedPrice.Value;
            return productDto;
        }

        [HttpPost]
        [Route("")]
        public async Task<ObjectResult> CreateProductCategory([FromBody] CreateProductCategoryDto createProductCategoryDto)
        {
            var productCategory = await _productCategoryFactory.Create();
            _mapper.Map(createProductCategoryDto, productCategory);
            _productCategoryRepository.Add(productCategory);
            await _appDbContext.SaveChangesAsync();
            var productCategoryDto = _mapper.Map<ProductCategoryDto>(productCategory);

            return new CreatedResult(new Uri("?id=" + productCategoryDto.Id, UriKind.Relative), productCategoryDto);
        }

        [HttpPut]
        [Route("")]
        public async Task<ObjectResult> ModifyProductCategory([FromBody] ModifyProductCategoryDto modifyProductCategoryDto)
        {
            var articleCategory = await _productCategoryRepository.FindSingleOrDefault(modifyProductCategoryDto.Id);
            _mapper.Map(modifyProductCategoryDto, articleCategory);
            _productCategoryRepository.Update(articleCategory);
            await _appDbContext.SaveChangesAsync();

            var articleCategoryDto = _mapper.Map<ProductCategoryDto>(articleCategory);

            return new ObjectResult(articleCategoryDto);
        }


        [HttpDelete]
        [Route("")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteProductCategoryPermanently([NoEmpty] Guid id)
        {
            _productCategoryRepository.Delete(id);
            _appDbContext.SaveChanges();

            return new OkResult();
        }

        [HttpPut]
        [Authorize]
        [Route("markdeleted")]
        public async Task<ObjectResult> MarkArticleAsDeleted([NoEmpty]Guid id, bool isDeleted = true)
        {
            var articleCategory = await _productCategoryRepository.FindSingleOrDefault(id);
            if (articleCategory == null) return new NotFoundObjectResult(null);

            if (isDeleted)
            {
                await _productCategoryRepository.MarkAsDeleted(id);
            }
            else
            {
                await _productCategoryRepository.MarkAsUnDeleted(id);
            }
            await _appDbContext.SaveChangesAsync();

            articleCategory = await _productCategoryRepository.FindSingleOrDefault(id);
            var articleCategoryDto = _mapper.Map<ProductCategoryDto>(articleCategory);

            return new OkObjectResult(articleCategoryDto);
        }
    }
}
