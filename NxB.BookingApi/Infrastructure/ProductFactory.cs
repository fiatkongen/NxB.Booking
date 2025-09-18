using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.AllocationApi;
using NxB.Clients.Interfaces;
using NxB.Dto.InventoryApi;
using NxB.Dto.PricingApi;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class ProductFactory
    {
        private readonly IClaimsProvider _claimsProvider;
        private readonly IMapper _mapper;
        private readonly IPriceProfileClient _priceProfileClient;

        public ProductFactory(IClaimsProvider claimsProvider, IMapper mapper, IPriceProfileClient priceProfileClient)
        {
            _claimsProvider = claimsProvider;
            _mapper = mapper;
            _priceProfileClient = priceProfileClient;
        }

        public async Task<Article> Create(decimal? fixedPrice)
        {
            var articleId = Guid.NewGuid();
            var article = new Article(articleId, _claimsProvider.GetTenantId());
            article.IsAvailableExtras = true;
            await CreateFixedPrice(fixedPrice, articleId);

            return article;
        }

        public async Task<Article> CreateProduct(decimal? fixedPrice)
        {
            var articleId = Guid.NewGuid();
            var article = new Article(articleId, _claimsProvider.GetTenantId());
            await CreateFixedPrice(fixedPrice, articleId);

            return article;
        }


        private async Task CreateFixedPrice(decimal? fixedPrice, Guid articleId)
        {
            await _priceProfileClient.Create(new CreatePriceProfileDto
                { ResourceId = articleId, Name = "standard", FixedPrice = fixedPrice });
        }
    }
}