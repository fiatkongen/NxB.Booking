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
    [Route("masterradio")]
    [Authorize]
    [ApiValidationFilter]
    public class MasterRadioController : BaseController
    {
        private readonly IMasterRadioRepository _masterRadioRepository;
        private readonly ITConService _tconService;
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;

        public MasterRadioController(ITConService tconService, AppDbContext appDbContext, IMapper mapper, IMasterRadioRepository masterRadioRepository)
        {
            _tconService = tconService;
            _appDbContext = appDbContext;
            _mapper = mapper;
            _masterRadioRepository = masterRadioRepository;
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAllRadiosAccesses()
        {
            var masterRadios = await _masterRadioRepository.FindAllMasterRadios();
            var masterRadioDtos = masterRadios.Select(x => _mapper.Map<MasterRadioDto>(x)).ToList();
            return new ObjectResult(masterRadioDtos);
        }
    }
}
