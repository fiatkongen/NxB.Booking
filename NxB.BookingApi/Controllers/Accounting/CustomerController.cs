using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using Newtonsoft.Json;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Dto;
using NxB.Dto.AccountingApi;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Munk.Utils.Object;
using NxB.Dto.OrderingApi;

namespace NxB.BookingApi.Controllers.Accounting
{
    [Produces("application/json")]
    [Route("customer")]
    [Authorize]
    [ApiValidationFilter]
    public class CustomerController : BaseController
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerTestDataImporter _customerTestDataImporter;
        private readonly CustomerFactory _customerFactory;
        private readonly AppDbContext _appDbContext;

        public CustomerController(ICustomerRepository customerRepository, CustomerFactory customerFactory, AppDbContext appDbContext, ICustomerTestDataImporter customerTestDataImporter)
        {
            _customerRepository = customerRepository;
            _customerFactory = customerFactory;
            _appDbContext = appDbContext;
            _customerTestDataImporter = customerTestDataImporter;
        }

        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public ObjectResult FindSingleCustomer(Guid id)
        {
            var customer = this._customerRepository.FindSingle(id, true);
            var customerDto = _customerFactory.Map(customer);
            return new ObjectResult(customerDto);
        }

        [HttpGet]
        [Route("account")]
        public ObjectResult FindSingleCustomerFromAccountId(Guid accountId)
        {
            var customer = this._customerRepository.FindSingleFromAccountId(accountId, true);
            var customerDto = _customerFactory.Map(customer);
            return new ObjectResult(customerDto);
        }

        [HttpGet]
        [Route("friendlyid")]
        public ObjectResult FindSingleCustomerFromFriendlyId(long friendlyId)
        {
            var customer = this._customerRepository.FindSingleOrDefaultFromFriendlyId(friendlyId);
            if (customer == null) return new ObjectResult(null);
            var customerDto = _customerFactory.Map(customer);
            return new ObjectResult(customerDto);
        }

        [HttpGet]
        [Route("onlinecustomer/friendlyorderid")]
        [AllowAnonymous]
        public async Task<ObjectResult> FindSingleOnlineCustomerFromFriendlyOrderId(long friendlyOrderId, Guid tenantId)
        {
            var orderClient = new OrderClient(null);
            await orderClient.AuthorizeClient(tenantId);
            var order = await orderClient.FindOrder(friendlyOrderId);
            if (order == null) return new ObjectResult(null);

            var customer = this._customerRepository.FindSingleFromAccountId(order.AccountId, tenantId);
            if (customer == null) return new ObjectResult(null);

            var onlineCustomerDto = MapToOnlineCustomerDto(customer);
            return new ObjectResult(onlineCustomerDto);
        }

        private static OnlineCustomerDto MapToOnlineCustomerDto(Customer customer)
        {
            var onlineCustomerDto = new OnlineCustomerDto
            {
                Address = customer.Address.Street,
                City = customer.Address.City,
                CountryId = customer.Address.CountryId,
                Zip = customer.Address.Zip,
                LicensePlate = customer.LicensePlateEntries.FirstOrDefault()?.Number,
                Email = customer.EmailEntries.OrderBy(x => x.ContactPriority).FirstOrDefault()?.Email,
                Phone = customer.PhoneEntries.FirstOrDefault()?.Number,
                Prefix = customer.PhoneEntries.FirstOrDefault()?.Prefix,
                Firstname = customer.Fullname.Firstname,
                Lastname = customer.Fullname.Lastname,
                ArrivalTime = DateTime.Now.ToEuTimeZone()
            };
            return onlineCustomerDto;
        }

        [HttpGet]
        [Route("onlinecustomer/orderid")]
        [AllowAnonymous]
        public async Task<ObjectResult> FindSingleOnlineCustomerFromOrderId(Guid orderId, Guid tenantId)
        {
            var orderClient = new OrderClient(null);
            await orderClient.AuthorizeClient(tenantId);
            var order = await orderClient.FindOrder(orderId);
            if (order == null) return new ObjectResult(null);

            var customer = this._customerRepository.FindSingleFromAccountId(order.AccountId, tenantId);
            if (customer == null) return new ObjectResult(null);

            var onlineCustomerDto = MapToOnlineCustomerDto(customer);
            return new ObjectResult(onlineCustomerDto);
        }

        [HttpPost]
        [Route("")]
        public async Task<ObjectResult> CreateCustomer([FromBody] CreateCustomerDto customerCreateDto)
        {
            var customer = await _customerFactory.Create(customerCreateDto);
            _customerRepository.Add(customer);
            await _appDbContext.SaveChangesAsync();
            var customerDto = _customerFactory.Map(customer);
            return new CreatedResult(new Uri("?id=" + customer.Id, UriKind.Relative), customerDto);
        }

        [HttpPut]
        [Route("")]
        public async Task<IActionResult> ModifyCustomer([FromBody] ModifyCustomerDto dto)
        {
            var customer = _customerRepository.FindSingle(dto.Id);
            await _customerFactory.Modify(dto, customer);
            _customerRepository.Update(customer);
            await _appDbContext.SaveChangesAsync();
            return new OkResult();
        }

        [HttpPost]
        [Route("import/testdata")]
        [Authorize(Roles = "Admin")]
        public async Task<ObjectResult> ImportCustomersTestData([NoEmpty]Guid destinationTenantId)
        {
            var allCustomers = await _customerRepository.FindAllIncludeDeleted();
            var friendlyCustomerIdMax = allCustomers.Count > 0 ? allCustomers.Max(x => x.FriendlyId) : 0;
            _customerFactory.TestId = friendlyCustomerIdMax + 1;
            var customersTestData = _customerTestDataImporter.BuildCustomersTestData(destinationTenantId);
            _customerRepository.Add(customersTestData);
            _appDbContext.SaveChanges();

            return StatusCode(200, new ImportResultDto { CreatedCount = customersTestData.Count });
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAllCustomers()
        {
            var customers = await _customerRepository.FindAll();
            var customerDtos = customers.Select(x => _customerFactory.Map(x));
            return new OkObjectResult(customerDtos);
        }

        [HttpGet]
        [Route("list/wildcard")]
        public async Task<ObjectResult> FindCustomersFromWildcard(string wildcard)
        {
            var customers = await _customerRepository.FindFromWildcard(wildcard);
            var customerDtos = customers.Select(x => _customerFactory.Map(x));
            return new OkObjectResult(customerDtos);
        }

        [HttpDelete]
        [Route("")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteCustomerPermanently([NoEmpty]Guid id)
        {
            _customerRepository.Delete(id);
            _appDbContext.SaveChanges();

            return new OkResult();
        }

        [HttpPut]
        [Authorize]
        [Route("markdeleted")]
        public IActionResult MarkCustomerAsDeleted(Guid id)
        {
            _customerRepository.MarkAsDeleted(id);
            _appDbContext.SaveChanges();

            return new OkResult();
        }

        [HttpPut]
        [Authorize]
        [Route("undelete")]
        public IActionResult Undelete([NoEmpty]Guid id)
        {
            var customer = _customerRepository.FindSingleIncludeDeleted(id);
            if (customer == null) return new NotFoundObjectResult(null);

            _customerRepository.Undelete(id);
            _appDbContext.SaveChanges();

            return new OkResult();
        }

        [HttpPost]
        [Route("note")]
        public IActionResult ModifyCustomerNote([FromBody]ModifyCustomerNoteDto modifyCustomerNoteDto)
        {
            this._customerRepository.UpdateCustomerNote(modifyCustomerNoteDto.CustomerId, modifyCustomerNoteDto.Note, modifyCustomerNoteDto.NoteState);
            this._appDbContext.SaveChanges();
            return Ok();
        }
    }
}
