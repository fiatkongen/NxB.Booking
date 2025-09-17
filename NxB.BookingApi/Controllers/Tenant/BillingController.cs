using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NxB.Domain.Common.Enums;
using NxB.Dto.AccountingApi;
using NxB.Dto.TenantApi;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Controllers.Tenant
{
    [Produces("application/json")]
    [Route("billing")]
    [Authorize]
    [ApiValidationFilter]
    public class BillingController : BaseController
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IBillableItemsRepository _billableItemsRepository;
        private readonly BillableItemFactory _billableItemFactory;

        public BillingController(AppDbContext appDbContext, IMapper mapper, IBillableItemsRepository billableItemsRepository, BillableItemFactory billableItemFactory)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _billableItemsRepository = billableItemsRepository;
            _billableItemFactory = billableItemFactory;
        }

        [HttpGet]
        [Route("")]
        public async Task<ObjectResult> FindSingleBillableItem(Guid id)
        {
            var billableItem = await _billableItemsRepository.FindSingleOrDefault(id);
            var billableItemDto =  _mapper.Map<BillableItemDto>(billableItem);
            return new OkObjectResult(billableItemDto);
        }

        [HttpGet]
        [Route("billeditemref")]
        public async Task<ObjectResult> FindSingleFromBillableItemRef(Guid billedItemRef)
        {
            var billableItem = await _billableItemsRepository.FindSingleFromBilledItemRefRefId(billedItemRef);
            var billableItemDto = _mapper.Map<BillableItemDto>(billableItem);
            return new OkObjectResult(billableItemDto);
        }

        [HttpPut]
        [Route("activate")]
        public async Task<IActionResult> ActivateBillableItem(Guid billedItemRef)
        {
            var billableItem = await _billableItemsRepository.FindSingleFromBilledItemRefRefId(billedItemRef);
            billableItem.Activate();
            await _appDbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPut]
        [Route("deliverystatus")]
        public async Task<IActionResult> SetDeliveryStatus(Guid billedItemRef, DeliveryStatus deliveryStatus)
        {
            var billableItem = await _billableItemsRepository.FindSingleFromBilledItemRefRefId(billedItemRef);
            billableItem.DeliveryStatus = deliveryStatus;
            await _appDbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> DeleteBillableItem(Guid billedItemRef)
        {
            var billableItem = await _billableItemsRepository.FindSingleFromBilledItemRefRefId(billedItemRef);
            _billableItemsRepository.Delete(billableItem);
            await _appDbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        [Route("")]
        [Authorize(Roles = "Admin")]
        public async Task<ObjectResult> CreateBillableItem([FromBody] CreateBillableItemDto createBillableItemDto)
        {
            var billableItem = _billableItemFactory.Create(createBillableItemDto.Number, createBillableItemDto.Price, createBillableItemDto.Type, createBillableItemDto.BilledItemRef);
            _mapper.Map(createBillableItemDto, billableItem);
            await _billableItemsRepository.Add(billableItem);
            await _appDbContext.SaveChangesAsync();

            var billableItemDto = _mapper.Map<BillableItemDto>(billableItem);
            return new CreatedResult(new Uri("?id=" + billableItem.Id, UriKind.Relative), billableItemDto);
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAllBillableItems()
        {
            var billableItems = await _billableItemsRepository.FindAll();
            var billableItemsDto = billableItems.Select(x =>_mapper.Map<BillableItemDto>(x)).ToList();
            return new OkObjectResult(billableItemsDto);
        }

        [HttpGet]
        [Route("list/all/unpaid")]
        public async Task<ObjectResult> FindAllUnpaidBillableItems()
        {
            var billableItems = await _billableItemsRepository.FindAllUnpaid();
            var billableItemsDto = billableItems.Select(x => _mapper.Map<BillableItemDto>(x)).ToList();
            return new OkObjectResult(billableItemsDto);
        }
    }
}