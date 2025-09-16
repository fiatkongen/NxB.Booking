using AutoMapper;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.PricingApi;
using NxB.BookingApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Infrastructure
{
    public class ProductCategoryFactory
    {
        private readonly IClaimsProvider _claimsProvider;

        public ProductCategoryFactory(IClaimsProvider claimsProvider)
        {
            _claimsProvider = claimsProvider;
        }

        public async Task<ProductCategory> Create()
        {
            var id = Guid.NewGuid();
            var articleCategory = new ProductCategory(id, _claimsProvider.GetTenantId(), _claimsProvider.GetUserId());

            return articleCategory;
        }
    }
}
