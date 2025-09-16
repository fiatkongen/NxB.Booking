using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NxB.Dto.AccountingApi;

namespace NxB.Dto.Clients
{
    public interface IPaymentCompletionClient
    {
        Task RemovePendingFromOrder(long friendlyOrderId);
        Task<List<PaymentCompletionDto>> FindPaymentCompletionsIncludePending_Global();
        Task<int> CountActiveTransactions(Guid? tenantId = null);
    }
}
