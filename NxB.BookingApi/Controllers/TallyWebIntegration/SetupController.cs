using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Itenso.TimePeriod;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.TallyWebIntegrationApi;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Controllers.TallyWebIntegration
{
    [Produces("application/json")]
    [Route("setup")]
    [Authorize]
    [ApiValidationFilter]
    public class SetupController : BaseController
    {
        private readonly ITConMasterRadioTenantMapRepository _tconMasterRadioTenantMapRepository;
        private readonly AppDbContext _appDbContext;
        private readonly IClaimsProvider _claimsProvider;
        private readonly ISetupRepository _setupRepository;
        private readonly IMapper _mapper;
        private readonly IMasterRadioRepository _masterRadioRepository;
        private readonly SetupAccessFactory _setupAccessFactory;
        private readonly SetupPeriodFactory _setupPeriodFactory;

        public SetupController(ITConMasterRadioTenantMapRepository tconMasterRadioTenantMapRepository, AppDbContext appDbContext, IClaimsProvider claimsProvider, ISetupRepository setupRepository, IMapper mapper, IMasterRadioRepository masterRadioRepository, SetupAccessFactory setupAccessFactory, SetupPeriodFactory setupPeriodFactory)
        {
            _appDbContext = appDbContext;
            _claimsProvider = claimsProvider;
            _setupRepository = setupRepository;
            _mapper = mapper;
            _masterRadioRepository = masterRadioRepository;
            _setupAccessFactory = setupAccessFactory;
            _setupPeriodFactory = setupPeriodFactory;
            _tconMasterRadioTenantMapRepository = tconMasterRadioTenantMapRepository;
        }

        [HttpPost]
        [Route("masterradiotenantmap")]
        public ObjectResult CreateTallyMasterRadioTenantMap([FromBody] CreateTallyMasterRadioTenantMapDto dto)
        {
            var tallyMasterRadioTenantMap = new TConMasterRadioTenantMap(Guid.NewGuid(), dto.TallyMasterRadioId, _claimsProvider.GetTenantId());
            _tconMasterRadioTenantMapRepository.Add(tallyMasterRadioTenantMap);
            _appDbContext.SaveChanges();
            return new CreatedResult(new Uri("?id=" + tallyMasterRadioTenantMap.Id, UriKind.Relative), tallyMasterRadioTenantMap);
        }

        [HttpGet]
        [Route("setupperiod/list/all")]
        public async Task<ObjectResult> FindAllSetupPeriods()
        {
            var setupPeriods = await _setupRepository.FindSetupPeriods();
            var setupPeriodsDtos = setupPeriods.Select(x => _mapper.Map<SetupPeriodDto>(x)).ToList();
            return new ObjectResult(setupPeriodsDtos);
        }

        [HttpPut]
        [Route("setupperiod")]
        public async Task<IActionResult> ModifySetupPeriod([FromBody] SetupPeriodDto setupPeriodDto)
        {
            var setupPeriods = await _setupRepository.FindSetupPeriods();
            var setupPeriod = setupPeriods.Single(x => x.No == setupPeriodDto.No);
            _mapper.Map(setupPeriodDto, setupPeriod);
            await _setupRepository.Update(setupPeriod);
            await _masterRadioRepository.PublishUpdate();
            return new OkResult();
        }

        [HttpPost]
        [Route("setupperiod")]
        public async Task<ObjectResult> AddSetupPeriod(int no)
        {
            if (no > 8 || no < 1) throw new SetupException("No skal være en værdi mellem 1-8");
            var setupPeriods = await _setupRepository.FindSetupPeriods();
            var setupPeriod = setupPeriods.SingleOrDefault(x => x.No == no);
            if (setupPeriod != null)
            {
                throw new SetupException($"Kan ikke oprette periode {no}, da den allerede eksisterer");
            }
            setupPeriod = _setupPeriodFactory.CreateDefault(no);

            await _setupRepository.Add(setupPeriod);
            await _masterRadioRepository.PublishUpdate();
            return new CreatedResult(new Uri("?id=" + setupPeriod.No, UriKind.Relative), setupPeriod);
        }

        [HttpDelete]
        [Route("setupperiod")]
        public async Task<IActionResult> RemoveSetupPeriod(int no)
        {
            if (no > 8 || no < 1) throw new SetupException("No skal være en værdi mellem 0-27");
            var setupPeriods = await _setupRepository.FindSetupPeriods();
            var setupPeriod = setupPeriods.SingleOrDefault(x => x.No == no);
            if (setupPeriod == null)
            {
                throw new SetupException($"Kan ikke slette periode {no}, da den ikke eksisterer");
            }
            setupPeriod = _setupPeriodFactory.CreateDefault(no);

            await _setupRepository.RemoveSetupPeriod(no);
            await _masterRadioRepository.PublishUpdate();
            return new OkResult();
        }

        [HttpGet]
        [Route("setupaccess/list/all")]
        public async Task<ObjectResult> FindAllSetupAccesses()
        {
            var setupAccesses = await _setupRepository.FindSetupAccesses();
            var setupAccessDtos = setupAccesses.Select(x => _mapper.Map<SetupAccessDto>(x)).ToList();
            return new ObjectResult(setupAccessDtos);
        }

        [HttpPut]
        [Route("setupaccess")]
        public async Task<IActionResult> ModifySetupAccess([FromBody] SetupAccessDto setupAccessDto)
        {
            var setupAccesses = await _setupRepository.FindSetupAccesses();
            var setupAccess = setupAccesses.Single(x => x.No == setupAccessDto.No);
            _mapper.Map(setupAccessDto, setupAccess);
            await _setupRepository.Update(setupAccess);
            await _masterRadioRepository.PublishUpdate();
            return new OkResult();
        }

        [HttpPost]
        [Route("setupaccess")]
        public async Task<ObjectResult> AddSetupAccess(int no)
        {
            if (no > 27 || no < 0) throw  new SetupException("No skal være en værdi mellem 0-27");
            var setupAccesses = await _setupRepository.FindSetupAccesses();
            var setupAccess = setupAccesses.SingleOrDefault(x => x.No == no);
            if (setupAccess != null)
            {
                throw new SetupException($"Kan ikke oprette adgang {no}, da den allerede eksisterer");
            }
            setupAccess = _setupAccessFactory.CreateDefault(no);

            await _setupRepository.Add(setupAccess);
            await _masterRadioRepository.PublishUpdate();
            return new CreatedResult(new Uri("?id=" + setupAccess.No, UriKind.Relative), setupAccess);
        }

        [HttpDelete]
        [Route("setupaccess")]
        public async Task<IActionResult> RemoveSetupAccess(int no)
        {
            if (no > 27 || no < 0) throw new SetupException("No skal være en værdi mellem 0-27");
            var setupAccesses = await _setupRepository.FindSetupAccesses();
            var setupAccess = setupAccesses.SingleOrDefault(x => x.No == no);
            if (setupAccess == null)
            {
                throw new SetupException($"Kan ikke slette adgang {no}, da den ikke eksisterer");
            }
            setupAccess = _setupAccessFactory.CreateDefault(no);

            await _setupRepository.RemoveSetupAccess(no);
            await _masterRadioRepository.PublishUpdate();
            return new OkResult();
        }
    }
}
