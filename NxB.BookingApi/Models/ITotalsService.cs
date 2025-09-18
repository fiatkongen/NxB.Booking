using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NxB.Dto.AccountingApi;
using NxB.Clients.Interfaces;

namespace NxB.BookingApi.Models
{
    public interface ITotalsService: IAuthorizeClient
    {
        Task<AccountTotalsDto> CalculateAccountTotals(Guid accountId);
        Task<AccountTotalsDto> CalculateOrderTotals(Guid accountId, Guid orderId);
    }
}
