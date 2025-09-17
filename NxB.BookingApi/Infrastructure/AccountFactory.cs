using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.AccountingApi;
using NxB.Dto.AllocationApi;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class AccountFactory
    {
        private readonly IClaimsProvider _claimsProvider;
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper = ConfigureMapper();

        public AccountFactory(IClaimsProvider claimsProvider, IAccountRepository accountRepository)
        {
            _claimsProvider = claimsProvider;
            _accountRepository = accountRepository;
        }

        //works but is not need right now
        //public async Task<Account> Create(Guid customerId, long friendlyCustomerId, Guid orderId, string name)
        //{
        //    if (customerId == Guid.Empty) throw new ArgumentException("Parameter cannot be empty", nameof(customerId));
        //    if (orderId == Guid.Empty) throw new ArgumentException("Parameter cannot be empty", nameof(orderId));
        //    if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Parameter cannot be empty", nameof(name));

        //    var nextIndexForAccount = await GetNextIndexForAccount(customerId);
        //    Account account = new Account(Guid.NewGuid(), ClaimsProvider.GetTenantId(), customerId, friendlyCustomerId , orderId, name, nextIndexForAccount);
        //    return account;
        //}
        public async Task<Account> CreateAccount(Guid customerId, Guid accountId, string accountName, long friendlyCustomerId)
        {
            return (await this.CreateAccounts(customerId, new List<(Guid, string)> { (accountId, accountName) }, friendlyCustomerId)).First();
        }

        public async Task<List<Account>> CreateAccounts(Guid customerId, List<(Guid, string)> accountNames, long friendlyCustomerId)
        {
            var nextIndexForAccount = await GetNextIndexForAccount(customerId);
            var accounts = accountNames.Select(x => new Account(x.Item1, _claimsProvider.GetTenantId(), customerId, friendlyCustomerId, x.Item2, nextIndexForAccount++)).ToList();
            return accounts;
        }

        public async Task<Account> CreateAccountFromDto(CreateAccountDto dto, long friendlyCustomerId)
        {
            var account = await this.CreateAccount(dto.CustomerId, Guid.NewGuid(), dto.Name, friendlyCustomerId);
            return account;
        }

        public async Task<List<Account>> CreateAccountsFromDtos(List<CreateAccountDto> dtos, long friendlyCustomerId)
        {
            if (dtos.Count == 0)
            {
                return new List<Account>();
            }
            var account = await this.CreateAccounts(dtos.First().CustomerId, dtos.Select(x => (Guid.NewGuid(), x.Name)).ToList(), friendlyCustomerId);
            return account;
        }

        private async Task<int> GetNextIndexForAccount(Guid customerId)
        {
            var accounts = await _accountRepository.FindAllForCustomerIncludeDeleted(customerId);
            int nextId = accounts.Count > 0 ? accounts.Max(x => x.Index) + 1 : 1;
            return nextId;
        }

        public AccountDto Map(Account model)
        {
            var dto = _mapper.Map<AccountDto>(model);
            return dto;
        }

        private static IMapper ConfigureMapper()
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CreateAccountDto, Account>();
                cfg.CreateMap<Account, CreateAccountDto>();

                cfg.CreateMap<AccountDto, Account>();
                cfg.CreateMap<Account, AccountDto>();

            }).CreateMapper();

            return mapper;
        }
    }
}