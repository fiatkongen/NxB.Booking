using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Dto.AccountingApi;
using NxB.Clients.Interfaces;
using ServiceStack;

namespace NxB.Clients
{
    public class PaymentCompletionClient : NxBAdministratorClient, IPaymentCompletionClient
    {
        public PaymentCompletionClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task<List<PaymentCompletionDto>> FindPaymentCompletionsIncludePending_Global()
        {
            var url = $"/NxB.Services.App/NxB.AccountingApi/paymentcompletion/list/all/includepending";
            var result = await this.GetAsync<List<PaymentCompletionDto>>(url);
            return result;
        }

        public async Task<int> CountActiveTransactions(Guid? tenantId = null)
        {
            var url = $"/NxB.Services.App/NxB.AccountingApi/paymentcompletion/count/active" + (tenantId != null ? "?tenantId=" + tenantId : "");
            var count = await this.GetAsync<int>(url);
            return count;
        }

        public async Task RemovePendingFromOrder(long friendlyOrderId)
        {
            var url = $"/NxB.Services.App/NxB.AccountingApi/paymentcompletion/pending/remove?friendlyOrderId=" + friendlyOrderId;
            await this.PutAsync(url, null);
        }

        //public async Task<PaymentCompletionDto> CreatePaymentCompletion(CreatePaymentCompletionDto createDto)
        //{
        //    var url = $"/NxB.Services.App/NxB.AccountingApi/paymentcompletion/pending/remove?friendlyOrderId=" + friendlyOrderId;
        //    return await this.PutAsync<PaymentCompletionDto>(url, null);
        //}
    }
}
