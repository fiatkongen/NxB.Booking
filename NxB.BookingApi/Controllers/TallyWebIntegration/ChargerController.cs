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
    [Route("charge")]
    [Authorize]
    [ApiValidationFilter]
    public class ChargerController : BaseController
    {
        private readonly ITConService _tconService;
        private readonly IMapper _mapper;
        private readonly AppTallyDbContext _appTallyDbContext;

        public ChargerController(ITConService tconService, IMapper mapper, AppTallyDbContext appTallyDbContext)
        {
            _tconService = tconService;
            _mapper = mapper;
            _appTallyDbContext = appTallyDbContext;
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAllChargers()
        {
            var sockets = await _tconService.FindAllChargers();
            var socketDtos = sockets.Select(x => _mapper.Map<ChargerDto>(x)).ToList();
            return new ObjectResult(socketDtos);
        }

        [HttpGet]
        [Route("consumption/list")]
        public async Task<ObjectResult> FindChargeConsumptions(int radioAddress, int socketNo)
        {
            var chargeConsumptions = await _tconService.FindChargeConsumptions(radioAddress, socketNo);
            var chargeConsumptionDtos = chargeConsumptions.Select(x => _mapper.Map<ChargeConsumptionDto>(x)).ToList();
            return new ObjectResult(chargeConsumptionDtos);
        }
    }
}
