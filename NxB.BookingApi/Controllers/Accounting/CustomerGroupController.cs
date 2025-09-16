using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.AccountingApi;

namespace NxB.BookingApi.Controllers.Accounting
{
    [Produces("application/json")]
    [Route("customergroup")]
    [Authorize]
    [ApiValidationFilter]
    public class CustomerGroupController : BaseController
    {
        private readonly AppDbContext _appDbContext;
        private readonly ICustomerGroupRepository _customerGroupRepository;
        private readonly IClaimsProvider _claimsProvider;

        public CustomerGroupController(ICustomerGroupRepository customerGroupRepository, AppDbContext appDbContext, IClaimsProvider claimsProvider)
        {
            _customerGroupRepository = customerGroupRepository;
            _appDbContext = appDbContext;
            _claimsProvider = claimsProvider;
        }

        [HttpGet]
        [Route("")]
        public ObjectResult FindSingleCustomerGroup(Guid id)
        {
            var customerGroup = this._customerGroupRepository.FindSingle(id);
            return new ObjectResult(customerGroup);
        }

        [HttpPost]
        [Route("")]
        public async Task<ObjectResult> CreateCustomerGroup([FromBody] CreateCustomerGroupDto dto)
        {
            var customerGroup = new CustomerGroup(_claimsProvider.GetTenantId(), dto.Name);
            _customerGroupRepository.Add(customerGroup);
            _appDbContext.SaveChanges();
            var customerGroupDto = MapToCustomerGroupDto(customerGroup);
            return new CreatedResult(new Uri("?id=" + customerGroupDto.Id, UriKind.Relative), customerGroupDto);
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindCustomerGroups()
        {
            var customerGroups = await this._customerGroupRepository.FindAll();
            var dtos = customerGroups.Select(MapToCustomerGroupDto).ToList();
            return new ObjectResult(dtos);
        }

        [HttpPut]
        [Route("markdeleted")]
        public IActionResult MarkCustomerAsDeleted(Guid id)
        {
            _customerGroupRepository.MarkAsDeleted(id);
            _appDbContext.SaveChanges();

            return new OkResult();
        }

        [HttpPut]
        [Route("rename")]
        public async Task<IActionResult> RenameCustomerGroup(Guid id, string newName)
        {
            var customerGroup = this._customerGroupRepository.FindSingle(id);
            customerGroup.Name = newName;
            await _appDbContext.SaveChangesAsync();

            return new OkResult();
        }

        private CustomerGroupDto MapToCustomerGroupDto(CustomerGroup customerGroup)
        {
            var customerGroupDto = new CustomerGroupDto
                { Id = customerGroup.Id, Name = customerGroup.Name, IsDeleted = customerGroup.IsDeleted };
            return customerGroupDto;
        }

    }
}
