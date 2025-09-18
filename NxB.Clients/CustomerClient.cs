using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Munk.Utils.Object;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.AccountingApi;
using NxB.Clients.Interfaces;

namespace NxB.Clients
{
    public class CustomerClient : NxBAdministratorClient, ICustomerClient
    {
        private readonly Dictionary<Guid, CustomerDto> _customerDtosCache = new();

        public CustomerClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task<CustomerDto> CreateCustomer(CreateCustomerDto createCustomerDto)
        {
            var url = $"/NxB.Services.App/NxB.AccountingApi/customer";
            var customerDto = await this.PostAsync<CustomerDto>(url, createCustomerDto);
            return customerDto;
        }

        public async Task<CustomerDto> FindCustomerFromAccountId(Guid accountId)
        {
            if (_customerDtosCache.ContainsKey(accountId))
            {
                return _customerDtosCache[accountId];
            }
            var url = $"/NxB.Services.App/NxB.AccountingApi/customer/account?accountId={accountId}";
            var customerDto = await this.GetAsync<CustomerDto>(url);
            this._customerDtosCache.Add(accountId, customerDto);
            return customerDto;
        }

        public async Task<CustomerDto> FindCustomer(Guid id)
        {
            if (_customerDtosCache.ContainsKey(id))
            {
                return _customerDtosCache[id];
            }
            var url = $"/NxB.Services.App/NxB.AccountingApi/customer?id={id}";
            var customerDto = await this.GetAsync<CustomerDto>(url);
            this._customerDtosCache.Add(id, customerDto);
            return customerDto;
        }

        public async Task<CustomerDto> FindCustomerFromFriendlyId(long friendlyId)
        {
            var url = $"/NxB.Services.App/NxB.AccountingApi/customer/friendlyid?friendlyid={friendlyId}";
            var customerDto = await this.GetAsync<CustomerDto>(url);
            return customerDto;
        }
    }
}
