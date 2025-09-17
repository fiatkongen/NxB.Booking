using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using Munk.Utils.Object;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;
using NxB.Dto.AccountingApi;
using NxB.Dto.Clients;

namespace NxB.BookingApi.Controllers.Accounting
{
    [Produces("application/json")]
    [Route("account")]
    [Authorize]
    [ApiValidationFilter]
    public class AccountController : BaseController
    {
        private readonly IAccountRepository _accountRepository;
        private readonly AccountFactory _accountFactory;
        private readonly AppDbContext _appDbContext;
        private readonly ICustomerRepository _customerRepository;
        private readonly ITotalsService _totalsService;
        private readonly IVoucherRepository _voucherRepository;
        private readonly IOrderRepository _orderRepository;

        public AccountController(IAccountRepository accountRepository, AccountFactory accountFactory, AppDbContext appDbContext, ICustomerRepository customerRepository, ITotalsService totalsService, IVoucherRepository voucherRepository, IOrderRepository orderRepository )
        {
            _accountRepository = accountRepository;
            _accountFactory = accountFactory;
            _appDbContext = appDbContext;
            _customerRepository = customerRepository;
            _totalsService = totalsService;
            _voucherRepository = voucherRepository;
            _orderRepository = orderRepository;
        }

        [HttpPost]
        [Route("")]
        public async Task<ObjectResult> CreateAccount([FromBody] CreateAccountDto createAccountDto)
        {
            var customer = _customerRepository.FindSingle(createAccountDto.CustomerId);
            var account = await _accountFactory.CreateAccountFromDto(createAccountDto, customer.FriendlyId);
            _accountRepository.Add(account);
            _appDbContext.SaveChanges();
            var accountDto = _accountFactory.Map(account);
            return new CreatedResult(new Uri("?id=" + account.Id, UriKind.Relative), accountDto);
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAllAccounts()
        {
            var accounts = await _accountRepository.FindAll();
            return new ObjectResult(accounts);
        }

        [HttpGet]
        [Route("")]
        public ObjectResult FindAccount(Guid id)
        {
            var account = _accountRepository.FindSingle(id);
            return new ObjectResult(account);
        }

        [HttpGet]
        [Route("customer/list/all")]
        public async Task<ObjectResult> FindAllAccountsForCustomer([NoEmpty]Guid customerId)
        {
            var accounts = await _accountRepository.FindAllForCustomer(customerId);
            return new ObjectResult(accounts);
        }

        //http://wiki2.e-conomic.dk/salg/kunder-kunder/forfalden-saldo
        [HttpGet]
        [Route("calculate/totals")]
        public async Task<ObjectResult> CalculateAccountTotals([NoEmpty]Guid accountId)
        {
            var accountTotals = await _totalsService.CalculateAccountTotals(accountId);
            var invoicesTotal = await this._voucherRepository.CalculateTotalFromAccountId(accountId);
            var ordersTotal = await _orderClient.CalculateAccountTotal(accountId);
            accountTotals.NotInvoiced = ordersTotal - invoicesTotal;
            return new ObjectResult(accountTotals);
        }

        [HttpGet]
        [Route("order/calculate/totals")]
        public async Task<ObjectResult> CalculateOrderTotals(Guid accountId, [NoEmpty]Guid orderId)
        {
            var accountTotals = await _totalsService.CalculateOrderTotals(accountId, orderId);
            var invoiceTotal = await this._voucherRepository.CalculateTotalFromOrderId(orderId);
            var orderTotal = await _orderClient.CalculateOrderTotal(orderId);
            accountTotals.NotInvoiced = orderTotal - invoiceTotal;
            return new ObjectResult(accountTotals);
        }
    }
}
