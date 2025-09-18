using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Domain.Common.Dto;
using NxB.Clients.Interfaces;
using NxB.Dto.PricingApi;

namespace NxB.Clients
{
    public class PriceProfileClient : NxBAdministratorClientWithTenantUrlLookup, IPriceProfileClient
    {
        private static readonly Dictionary<string, PriceProfileDto> CachedDtos = new();

        public PriceProfileClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task<PriceProfileDto> FindSingle(Guid id)
        {
            var url = $"/NxB.Services.App/NxB.PricingApi/priceprofile?id=" + id;
            var priceProfileDto = await this.GetAsync<PriceProfileDto>(url);
            return priceProfileDto;
        }

        public async Task<PriceProfileDto> FindSingleOrDefaultFromResourceId(Guid resourceId, string ppName = "standard")
        {
            var url = $"/NxB.Services.App/NxB.PricingApi/priceprofile/resourceid?resourceId={resourceId}&ppName={ppName}";
            var priceProfileDto = await this.GetAsync<PriceProfileDto>(url);
            return priceProfileDto;
        }

        public async Task<List<PriceProfileDto>> FindAll(bool includeDeleted)
        {
            var url = $"/NxB.Services.App/NxB.PricingApi/priceprofile/list/all?includeDeleted=" + includeDeleted;
            var priceProfileDtos = await this.GetAsync<List<PriceProfileDto>>(url);
            return priceProfileDtos;
        }

        public async Task<List<PriceProfileDto>> FindAllFromTenantId(Guid tenantId, bool includeDeleted)
        {
            var url = $"/NxB.Services.App/NxB.PricingApi/priceprofile/list/all/tenant?tenantId={tenantId}&includeDeleted=" + includeDeleted;
            var priceProfileDtos = await this.GetAsync<List<PriceProfileDto>>(url);
            return priceProfileDtos;
        }

        public async Task<ImportResultDto> ConvertToFixed()
        {
            var url = $"/NxB.Services.App/NxB.PricingApi/priceprofile/convert/all/tofixedprice";
            var importResultDto = await this.PostAsync<ImportResultDto>(url, null);
            return importResultDto;
        }

        public async Task<PriceProfileDto> Create(CreatePriceProfileDto dto)
        {
            var url = $"/NxB.Services.App/NxB.PricingApi/priceprofile";
            var priceProfileDto = await this.PostAsync<PriceProfileDto>(url, dto);
            return priceProfileDto;
        }

        public async Task<PriceProfileDto> Delete(Guid id)
        {
            var url = $"/NxB.Services.App/NxB.PricingApi/priceprofile?id=" + id;
            var priceProfileDto = await this.DeleteAsync<PriceProfileDto>(url);
            return priceProfileDto;
        }

        public async Task DeleteForResource(Guid resourceId)
        {
            var url = $"/NxB.Services.App/NxB.PricingApi/priceprofile/resource?resourceId=" + resourceId;
            await this.DeleteAsync<PriceProfileDto>(url);
        }

        public async Task<PriceProfileDto> ModifyFixedPrice(Guid priceProfileId, decimal fixedPrice)
        {
            var url = $"/NxB.Services.App/NxB.PricingApi/priceprofile/modify/fixedprice?priceProfileId={priceProfileId}&fixedPrice={fixedPrice}";
            var priceProfileDto = await this.PutAsync<PriceProfileDto>(url, null);
            return priceProfileDto;
        }
    }
}
