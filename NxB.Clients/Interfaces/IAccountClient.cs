using System;
using System.Threading.Tasks;
using NxB.Dto.AccountingApi;

namespace NxB.Clients.Interfaces
{
    public interface IAccountClient
    {
        Task<AccountTotalsDto> CalculateOrderTotals(Guid accountId, Guid orderId);
    }
}