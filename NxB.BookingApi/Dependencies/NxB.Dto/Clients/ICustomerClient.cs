using System;
using System.Threading.Tasks;
using NxB.Dto.AccountingApi;

namespace NxB.Dto.Clients
{
    public interface ICustomerClient : IAuthorizeClient
    {
        Task<CustomerDto> CreateCustomer(CreateCustomerDto createCustomerDto);
        Task<CustomerDto> FindCustomerFromAccountId(Guid accountId);
        Task<CustomerDto> FindCustomerFromFriendlyId(long friendlyId);
        Task<CustomerDto> FindCustomer(Guid id);
    }
}
