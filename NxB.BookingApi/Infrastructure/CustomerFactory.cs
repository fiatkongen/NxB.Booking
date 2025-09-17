using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using NxB.Domain.Common.Constants;
using NxB.Domain.Common.Dto;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;
using NxB.Dto.AccountingApi;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class CustomerFactory
    {
        private readonly IClaimsProvider _claimsProvider;
        private readonly IFriendlyAccountingIdProvider _friendlyIdProvider;
        private readonly AccountFactory _accountFactory;
        private readonly ICountryRepository _countryRepository;
        private readonly IMapper _mapper;
        public long TestId = 1;

        public CustomerFactory(IClaimsProvider claimsProvider, IFriendlyAccountingIdProvider friendlyIdProvider, AccountFactory accountFactory, IMapper mapper, ICountryRepository countryRepository)
        {
            _claimsProvider = claimsProvider;
            _friendlyIdProvider = friendlyIdProvider;
            _accountFactory = accountFactory;
            _mapper = mapper;
            _countryRepository = countryRepository;
        }

        public async Task<Customer> Create()
        {
            var friendlyId = _friendlyIdProvider.GenerateNextFriendlyCustomerId();
            var customer = new Customer(Guid.NewGuid(), _claimsProvider.GetTenantId(), friendlyId);
            await AddAccounts(customer, new List<(Guid, string)> { (Guid.NewGuid(), AppConstants.DEFAULT_ACCOUNT_NAME) });
            return customer;
        }

        private async Task AddAccounts(Customer customer, List<(Guid, string)> accountNames)
        {
            var accounts = await _accountFactory.CreateAccounts(customer.Id, accountNames, customer.FriendlyId);
            customer.Accounts.AddRange(accounts);
        }

        public async Task<Customer> Create(CreateCustomerDto dto)
        {
            var customer = await this.Create();
            _mapper.Map(dto, customer);

            if (dto.NewAccounts.Count > 0)
            {
                customer.Accounts = new List<Account>();
            }

            await AddToCustomerChildCollections(dto, customer);
            return customer;
        }

        public async Task<Customer> CreateTestCustomer(CreateCustomerDto dto, Guid destinationTenantId)
        {
            var customer = new Customer(Guid.NewGuid(), destinationTenantId, TestId++);
            await AddAccounts(customer, new List<(Guid, string)> { (Guid.NewGuid(), AppConstants.DEFAULT_ACCOUNT_NAME) });

            _mapper.Map(dto, customer);

            if (dto.NewAccounts.Count > 0)
            {
                customer.Accounts = new List<Account>();
            }

            await AddToCustomerChildCollections(dto, customer);
            return customer;
        }

        private async Task AddToCustomerChildCollections(CreateCustomerDto dto, Customer customer)
        {
            await AddAccounts(customer, dto.NewAccounts.Select(x => (Guid.NewGuid(), x.Name)).ToList());
            Country country = null;
            if (customer.Address.CountryId != null)
            {
                country = await _countryRepository.FindSingle(customer.Address.CountryId);
            }

            customer.EmailEntries.AddRange(dto.NewEmailEntries.Select(x => _mapper.Map<EmailEntry>(x)).ToList());
            customer.PhoneEntries.AddRange(dto.NewPhoneEntries.Select(x =>
            {
                var dto = _mapper.Map<PhoneEntry>(x);

                if (string.IsNullOrWhiteSpace(dto.Prefix) && country != null)
                {
                    dto.Prefix = "+" + country.PhonePrefix;
                }

                if (dto.Number.IndexOf("00") == 0)
                {
                    dto.Number = "+" + dto.Number.Substring(2);
                }

                if (dto.Prefix.IndexOf("00") == 0)
                {
                    dto.Prefix = "+" + dto.Prefix.Substring(2);
                }


                if (dto.Prefix.Contains("+") && dto.Number.IndexOf(dto.Prefix) == 0)
                {
                    dto.Number = dto.Number.Replace(dto.Prefix + " ", "");    //user added prefix twice
                    dto.Number = dto.Number.Replace(dto.Prefix + "", "");
                }

                return dto;
            }).ToList());
            customer.LicensePlateEntries.AddRange(dto.NewLicensePlateEntries.Select(x => _mapper.Map<LicensePlateEntry>(x)).ToList());
            customer.CustomerGroupCustomers.AddRange(dto.NewCustomerGroupCustomers.Select(x => new CustomerGroupCustomer(customer.Id, x.CustomerGroupId)));
            customer.FamilyMembers.AddRange(dto.NewFamilyMembers.Select(x => _mapper.Map<FamilyMember>(x)).ToList());
        }

        public async Task Modify(ModifyCustomerDto dto, Customer customer)
        {
            _mapper.Map(dto, customer);
            await AddToCustomerChildCollections(dto, customer);
            ModifyCustomerChildCollections(dto, customer);
        }

        private void ModifyCustomerChildCollections(ModifyCustomerDto dto, Customer customer)
        {
            customer.EmailEntries.ForEach(ee =>
            {
                var modifiedDto = dto.ModifiedEmailEntries.SingleOrDefault(mee => mee.Id == ee.Id);
                if (modifiedDto != null)
                {
                    _mapper.Map(modifiedDto, ee);
                }
            });

            customer.PhoneEntries.ForEach(pe =>
            {
                var modifiedDto = dto.ModifiedPhoneEntries.SingleOrDefault(mpe => mpe.Id == pe.Id);
                if (modifiedDto != null)
                {
                    _mapper.Map(modifiedDto, pe);
                }
            });

            customer.LicensePlateEntries.ForEach(pe =>
            {
                var modifiedDto = dto.ModifiedLicensePlateEntries.SingleOrDefault(mlpe => mlpe.Id == pe.Id);
                if (modifiedDto != null)
                {
                    _mapper.Map(modifiedDto, pe);
                }
            });

            customer.CustomerGroupCustomers.ForEach(cgc =>
            {
                var removedDto = dto.DeletedCustomerGroupCustomers.SingleOrDefault(dcgc => dcgc.CustomerGroupId == cgc.CustomerGroupId);
                if (removedDto != null)
                {
                    cgc.IsDeleted = true;
                }
            });

            customer.FamilyMembers.ForEach(fm =>
            {
                var modifiedDto = dto.ModifiedFamilyMembers.SingleOrDefault(mfm => mfm.Id == fm.Id);
                if (modifiedDto != null)
                {
                    _mapper.Map(modifiedDto, fm);
                }
            });
        }

        public CustomerDto Map(Customer model)
        {
            //model.PhoneEntries = model.PhoneEntries.OrderBy(x => x.Index).ToList();
            //model.EmailEntries = model.EmailEntries.OrderBy(x => x.Index).ToList();
            //model.LicensePlateEntries = model.LicensePlateEntries.OrderBy(x => x.Index).ToList();
            model.Accounts = model.Accounts.OrderBy(x => x.Index).ToList();
            //model.FamilyMembers = model.FamilyMembers.OrderBy(x => x.Index).ToList();

            var customerDto = _mapper.Map<CustomerDto>(model);
            return customerDto;
        }
    }
}