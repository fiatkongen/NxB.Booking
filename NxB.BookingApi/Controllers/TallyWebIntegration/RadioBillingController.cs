using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using NxB.Dto.TallyWebIntegrationApi;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Controllers.TallyWebIntegration
{
    [Produces("application/json")]
    [Route("radiobilling")]
    [Authorize]
    [ApiValidationFilter]
    public class RadioBillingController : BaseController
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IRadioBillingRepository _radioBillingRepository;
        private readonly RadioBillingFactory _radioBillingFactory;

        public RadioBillingController(AppDbContext appDbContext, IMapper mapper, IRadioBillingRepository radioBillingRepository, RadioBillingFactory radioBillingFactory)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _radioBillingRepository = radioBillingRepository;
            _radioBillingFactory = radioBillingFactory;
        }

        [HttpPost]
        [Route("")]
        public async Task<ObjectResult> CreateRadioBilling([FromBody] RadioBillingDto dto)
        {
            var radioBilling = _radioBillingFactory.Create();
            _mapper.Map(dto, radioBilling);
            await _radioBillingRepository.Add(radioBilling);
            await _appDbContext.SaveChangesAsync();
            return new CreatedResult(new Uri("?id=" + radioBilling.RadioAddress, UriKind.Relative), radioBilling);
        }

        [HttpPut]
        [Route("")]
        public async Task<ObjectResult> ModifyRadioBilling([FromBody] RadioBillingDto dto)
        {
            var radioBilling = await _radioBillingRepository.FindSingle(dto.RadioAddress);
            _mapper.Map(dto, radioBilling);
            _radioBillingRepository.Update(radioBilling);
            await _appDbContext.SaveChangesAsync();
            var radioBillingDto = _mapper.Map<RadioBillingDto>(radioBilling);
            return new OkObjectResult(radioBillingDto);
        }

        [HttpDelete]
        [Route("")]
        public async Task<IActionResult> DeleteRadioBilling(int radioAddress)
        {
            await _radioBillingRepository.Delete(radioAddress);
            await _appDbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        [Route("")]
        public async Task<ObjectResult> FindRadioBilling(int radioAddress)
        {
            var radioBilling = await _radioBillingRepository.FindSingleOrDefault(radioAddress);
            if (radioBilling == null) return new ObjectResult(null);
            var radioBillingDto = _mapper.Map<RadioBillingDto>(radioBilling);
            return new ObjectResult(radioBillingDto);
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAllRadioBillings()
        {
            var radioBillings = await _radioBillingRepository.FindAll();
            var radioBillingDtos = radioBillings.Select(x => _mapper.Map<RadioBillingDto>(x));
            return new ObjectResult(radioBillingDtos);
        }
    }
}
