using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using Newtonsoft.Json;
using NxB.Dto.TallyWebIntegrationApi;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Controllers.TallyWebIntegration
{
    [Produces("application/json")]
    [Route("radio")]
    [Authorize]
    [ApiValidationFilter]
    public class RadioController : BaseController
    {
        private readonly ITConService _tconService;
        private readonly AppTallyDbContext _appTallyDbContext;
        private readonly IMapper _mapper;

        public RadioController(ITConService tconService, IMapper mapper, AppTallyDbContext appTallyDbContext)
        {
            _tconService = tconService;
            _mapper = mapper;
            _appTallyDbContext = appTallyDbContext;
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAllRadios()
        {
            var radios = await _tconService.FindAllRadios();
            var radioDtos = radios.Select(x => _mapper.Map<RadioDto>(x)).ToList();
            return new ObjectResult(radioDtos);
        }


        [HttpGet]
        [Route("socket/list/all")]
        public async Task<ObjectResult> FindAllSocketRadios()
        {
            var radios = await _tconService.FindAllSocketRadios();
            var radioDtos = radios.Select(x => _mapper.Map<RadioDto>(x)).ToList();
            return new ObjectResult(radioDtos);
        }

        [HttpPost]
        [Route("accesscodes/forceupdate")]
        public async Task<IActionResult> ForceAccessCodesUpdate(int radioAddress)
        {
            var radio = await _tconService.FindSingleRadio(radioAddress);
            radio.ForceUpdateOfAccessCodes();
            await _appTallyDbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
