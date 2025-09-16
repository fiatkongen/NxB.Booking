using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using Munk.Utils.Object;
using NxB.Dto.OrderingApi;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Controllers.Ordering
{
    [Produces("application/json")]
    [Route("discount")]
    [Authorize]
    [ApiValidationFilter]
    public class DiscountController : Controller
    {
        private readonly IDiscountRepository _discountRepository;
        private readonly DiscountFactory _discountFactory;
        private readonly AppDbContext _appDbContext;

        public DiscountController(IDiscountRepository discountRepository, DiscountFactory discountFactory, AppDbContext appDbContext)
        {
            _discountRepository = discountRepository;
            _discountFactory = discountFactory;
            _appDbContext = appDbContext;
        }

        [HttpPost]
        [Route("")]
        public async Task<ObjectResult> CreateDiscount([FromBody] CreateDiscountDto dto)
        {
            var discount = _discountFactory.Create(dto);
            _discountRepository.Add(discount);
            _appDbContext.SaveChanges();
            var discountDto = _discountFactory.Map(discount);
            var createdResult = new CreatedResult(new Uri("?id=" + discountDto.Id, UriKind.Relative), discountDto);
            return createdResult;
        }

        [HttpPut]
        [Route("")]
        public async Task<ObjectResult> UpdateDiscount([FromBody] DiscountDto discountDto)
        {
            var discount = _discountFactory.Map(discountDto);
            _discountRepository.Update(discount);
            _appDbContext.SaveChanges();
            var dto = _discountFactory.Map(discount);
            return new ObjectResult(dto);
        }

        [HttpPut]
        [Authorize]
        [Route("markdeleted")]
        public IActionResult MarkDiscountAsDeleted([NoEmpty]Guid id)
        {
            var discount = _discountRepository.FindSingle(id);
            if (discount == null) return new NotFoundObjectResult(null);

            _discountRepository.MarkAsDeleted(id);
            _appDbContext.SaveChanges();

            return new OkResult();
        }

        [HttpGet]
        [Route("")]
        public ObjectResult FindSingleDiscountOrDefault(Guid id)
        {
            var discount = _discountRepository.FindSingle(id);
            var dto = _discountFactory.Map(discount);
            return new ObjectResult(dto);
        }

        [HttpGet]
        [Route("name")]
        public ObjectResult FindSingleDiscountFromName(string name)
        {
            var discount = _discountRepository.FindSingleOrDefaultFromName(name);
            if (discount == null) return new ObjectResult(null);
            var dto = _discountFactory.Map(discount);
            return new ObjectResult(dto);
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAllAccounts()
        {
            var discounts = await _discountRepository.FindAll();
            return new ObjectResult(discounts);
        }
    }
}
