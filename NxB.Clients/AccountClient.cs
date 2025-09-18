using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.AccountingApi;
using NxB.Clients.Interfaces;

namespace NxB.Clients
{
    public class AccountClient : NxBAdministratorClient, IAccountClient
    {
        public AccountClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task<AccountTotalsDto> CalculateOrderTotals(Guid accountId, Guid orderId)
        {
            var url = $"/NxB.Services.App/NxB.AccountingApi/account/order/calculate/totals?accountId={accountId}&orderId={orderId}";
            var customerDto = await this.GetAsync<AccountTotalsDto>(url);
            return customerDto;
        }
    }
}
