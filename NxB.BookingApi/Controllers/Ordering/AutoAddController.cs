using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using NxB.Dto.AccountingApi;
using NxB.Dto.OrderingApi;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Controllers.Ordering
{
    [Produces("application/json")]
    [Route("autoadd")]
    [Authorize]
    [ApiValidationFilter]
    public class AutoAddController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAutoAddRepository _autoAddRepository;
        private readonly IMapper _mapper;
        private readonly AutoAddFactory _autoAddFactory;

        public AutoAddController(AppDbContext appDbContext, IAutoAddRepository autoAddRepository, IMapper mapper, AutoAddFactory autoAddFactory)
        {
            _appDbContext = appDbContext;
            _autoAddRepository = autoAddRepository;
            _mapper = mapper;
            _autoAddFactory = autoAddFactory;
        }

        [HttpGet]
        [Route("")]
        public ObjectResult FindSingleAutoAdd(Guid id)
        {
            var autoAdd = this._autoAddRepository.FindSingle(id);
            return new ObjectResult(autoAdd);
        }

        [HttpPost]
        [Route("")]
        public async Task<ObjectResult> CreateAutoAdd([FromBody] CreateAutoAddDto createDto)
        {
            var autoAdd = _autoAddFactory.Create(createDto);
            _autoAddRepository.Add(autoAdd);
            _appDbContext.SaveChanges();
            autoAdd = _autoAddRepository.FindSingle(autoAdd.Id);
            var dto = _mapper.Map<AutoAddDto>(autoAdd);
            return new CreatedResult(new Uri("?id=" + dto.Id, UriKind.Relative), dto);
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAutoAdds()
        {
            var autoAdds = await this._autoAddRepository.FindAll();
            var dtos = autoAdds.Select(x => _mapper.Map<AutoAddDto>(x)).ToList();
            return new ObjectResult(dtos);
        }

        [HttpDelete]
        [Route("")]
        public IActionResult DeleteAutoAdd(Guid id)
        {
            _autoAddRepository.Delete(id);
            _appDbContext.SaveChanges();

            return new OkResult();
        }

    }
}
