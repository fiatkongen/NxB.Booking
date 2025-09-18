using System;
using System.Threading.Tasks;
using NxB.Dto.AccountingApi;

namespace NxB.Clients
{
    public interface IAccountClient
    {
        Task<AccountTotalsDto> CalculateOrderTotals(Guid accountId, Guid orderId);
    }
}