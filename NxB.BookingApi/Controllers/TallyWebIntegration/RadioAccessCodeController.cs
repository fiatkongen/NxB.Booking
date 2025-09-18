using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using NxB.Domain.Common.Constants;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;
using NxB.Dto.TallyWebIntegrationApi;
// TODO: Remove Service Fabric dependency when migration is complete
// using NxB.Remoting.Interfaces.TallyWebIntegrationApi;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Controllers.TallyWebIntegration
{
    [Produces("application/json")]
    [Route("radioaccesscode")]
    [Authorize]
    [ApiValidationFilter]
    public class RadioAccessCodeController : BaseController
    {
        private readonly ITConService _tconService;
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly AppTallyDbContext _appTallyDbContext;
        private readonly IKeyCodeGenerator _keyCodeGenerator;
        private readonly IAccessGroupRepository _accessGroupRepository;
        private readonly IMasterRadioIdProvider _masterRadioIdProvider;

        public RadioAccessCodeController(ITConService tconService, AppDbContext appDbContext, IMapper mapper, AppTallyDbContext appTallyDbContext, IKeyCodeGenerator keyCodeGenerator, IAccessGroupRepository accessGroupRepository, IMasterRadioIdProvider masterRadioIdProvider)
        {
            _tconService = tconService;
            _appDbContext = appDbContext;
            _mapper = mapper;
            _appTallyDbContext = appTallyDbContext;
            _keyCodeGenerator = keyCodeGenerator;
            _accessGroupRepository = accessGroupRepository;
            _masterRadioIdProvider = masterRadioIdProvider;
        }

        [HttpPost]
        [Route("")]
        public async Task<ObjectResult> CreateAccessToRadios([FromBody] CreateRadioAccessDto createRadioAccessDto)
        {
            if (createRadioAccessDto.Code == null)
            {
                throw new AccessCodeException("Code cannot be null");
            }

            var code = (int)createRadioAccessDto.Code;

            var existingRadioCodes = await _tconService.FindAllRadioAccessCodesWithCode(code);
            List<int> radiosAdded = existingRadioCodes.Select(x => x.RadioAddress).ToList();

            var radioAccessCodes = await this._tconService.AddAccessBasedOnRadiosFilter(code, createRadioAccessDto.IsKeyCode, createRadioAccessDto.Option, createRadioAccessDto, radiosAdded);
            await _appTallyDbContext.SaveChangesAsync();

            var radioAccessNames = radioAccessCodes.Select(x => x.RadioAddress + "-" + x.Option).ToList();
            return new ObjectResult(radioAccessNames);
        }

        [HttpPost]
        [Route("accessibleitems")]
        public async Task<int> CreateAccessFromAccessibleItems([FromBody] CreateRadioAccessFromAccessibleItemsDto createDto)
        {
            if (createDto.AccessibleItems == null || createDto.AccessibleItems.IsEmpty)
            {
                throw new RadioAccessException($"Kan ikke oprette adgang for kort / kode: {createDto.Code}, da der ikke er valgt nogen accessibleItems. Er der oprettet adgangsgrupper for pladsen?");
            }

            var code = (int)createDto.Code;

            var tconService = GetTConService(createDto.TenantId);

            var existingRadioCodes = await tconService.FindAllRadioAccessCodesWithCode(code);
            List<int> radiosAdded = existingRadioCodes.Select(x => x.RadioAddress).ToList();

            foreach (var accessGroupId in createDto.AccessibleItems.AccessItems.Select(x => x.AccessGroupId).ToList())
            {
                var accessGroup = await AccessGroupRepository.FindSingle(accessGroupId, _appDbContext);
                var radioCodes = await tconService.AddAccessBasedOnRadiosFilter(code, createDto.IsKeyCode, accessGroup.Option, accessGroup, radiosAdded);
                radiosAdded.AddRange(radioCodes.Select(x => x.RadioAddress));
            }

            await _appTallyDbContext.SaveChangesAsync();
            return radiosAdded.Count;
        }

        private ITConService GetTConService(Guid? tenantId)
        {
            if (tenantId.HasValue)
            {
                return CreateTConServiceWithCustomClaimsProvider(tenantId.Value);
            }
            else
            {
                return _tconService;
            }
        }


        [HttpPut]
        [Route("accessibleitems")]
        public async Task<IActionResult> ModifyAccessFromAccessibleItems([FromBody] CreateRadioAccessFromAccessibleItemsDto modifyDto)
        {
            if (modifyDto.AccessibleItems == null || (modifyDto.AccessibleItems.AccessItems.Count == 0 && modifyDto.AccessibleItems.SwitchItems.Count == 0)) throw new RadioAccessException("RadioAccessController.CreateAccessFromAccessibleItems no accessgroups or switches specified.");
            int code = (int)modifyDto.Code;

            List<RadioAccessCode> existingRadioCodes = await _tconService.FindAllRadioAccessCodesWithCode(code);
            List<RadioAccessCode> radioCodesAdded = new List<RadioAccessCode>();
            List<int> radiosAdded = new List<int>();

            foreach (var accessGroupId in modifyDto.AccessibleItems.AccessItems.Select(x => x.AccessGroupId).ToList())
            {
                var accessGroup = await _accessGroupRepository.FindSingle(accessGroupId);
                var radioCodes = await this._tconService.AddAccessBasedOnRadiosFilter(code, modifyDto.IsKeyCode, accessGroup.Option, accessGroup, radiosAdded);
                radioCodesAdded.AddRange(radioCodes);
                radiosAdded.AddRange(radioCodes.Select(x => x.RadioAddress));
            }

            foreach (var addedCode in radioCodesAdded)
            {
                if (existingRadioCodes.Any(x => x.RadioAddress == addedCode.RadioAddress))
                {
                    _appTallyDbContext.Remove(addedCode._tconRadioAccessCode);
                }
            }

            foreach (var existingCode in existingRadioCodes)
            {
                if (radioCodesAdded.None(x => x.RadioAddress == existingCode.RadioAddress))
                {
                    await _tconService.RemoveRadioAccessCodeFromSingleRadio(existingCode.RadioAddress, existingCode.Code);
                }
            }


            await _appTallyDbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        [Route("count")]
        public async Task<ObjectResult> CountRadioAccessesForCode(uint code)
        {
            var existingRadioCodes = await _tconService.FindAllRadioAccessCodesWithCode((int)code);
            return new ObjectResult(new Dictionary<string, int> { { "result", existingRadioCodes.Count } });
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAllRadioAccessCodes()
        {
            var radioAccessCodes = await _tconService.FindAllRadioAccessCodes();
            var radioAccessCodeDtos = radioAccessCodes.Select(x => _mapper.Map<RadioAccessCodeDto>(x)).ToList();
            return new ObjectResult(radioAccessCodeDtos);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("switch/add/all")]
        public async Task<ObjectResult> AddAllCardsAndCodesToRadio(int radioAddress, int option)
        {
            var radioAccessCodes = await _tconService.FindAllRadioAccessCodes();
            var uniqueAccessCodes = radioAccessCodes.Select(x => new { x.Code, x.IsKeyCode }).Distinct();

            List<RadioAccessCode> newCodes = new List<RadioAccessCode>();

            foreach (var uniqueAccessCode in uniqueAccessCodes)
            {
                var existingAccessCode = await _tconService.FindRadioAccessCodeOrDefault(radioAddress, uniqueAccessCode.Code);
                if (existingAccessCode != null)
                {
                    //do nothing
                }
                else
                {
                    var addedCodes = await _tconService.AddAccessToSpecificSwitches(uniqueAccessCode.Code, uniqueAccessCode.IsKeyCode,
                        new List<RadioAccessUnit> { new() { Option = option, RadioAddress = radioAddress } });

                    newCodes = newCodes.Concat(addedCodes).ToList();
                }
                Debug.WriteLine("AccessCount: " + newCodes.Count);
            }

            var radioAccessCodeDtos = newCodes.Select(x => _mapper.Map<RadioAccessCodeDto>(x)).ToList();
            await _appTallyDbContext.SaveChangesAsync();
            return new ObjectResult(radioAccessCodeDtos);
        }

        [HttpGet]
        [Route("filter/code")]
        public async Task<ObjectResult> FindAllRadioAccessCodesForCode(uint code)
        {
            var radioAccessCodes = await _tconService.FindAllRadioAccessCodesWithCode((int)code);
            var radioAccessCodeDtos = radioAccessCodes.Select(x => _mapper.Map<RadioAccessCodeDto>(x)).ToList();
            return new ObjectResult(radioAccessCodeDtos);
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        [Route("all")]
        public async Task<ObjectResult> DeleteAllRadioAccessCodes()
        {
            var radioAccessCodes = await _tconService.RemoveAllRadioAccessCodes();
            var radioAccessCodeDtos = radioAccessCodes.Select(x => _mapper.Map<RadioAccessCodeDto>(x)).ToList();
            await _appTallyDbContext.SaveChangesAsync();
            return new ObjectResult(radioAccessCodeDtos);
        }

        [HttpDelete]
        [Route("")]
        public async Task<IActionResult> DeleteRadioAccessCodeFromId(int id)
        {
            await _tconService.RemoveRadioAccessCodeWithId(id);
            await _appTallyDbContext.SaveChangesAsync();
            return new OkResult();
        }

        [HttpDelete]
        [Route("code")]
        public async Task<IActionResult> DeleteRadioAccessCodeFromCode(uint code, bool markAsSettled)
        {
            await _tconService.RemoveRadioAccessCodesFromAllRadiosWithCode((int)code);
            if (markAsSettled)
            {
                await _tconService.MarkCodeAsSettled((int)code);
            }
            await _appTallyDbContext.SaveChangesAsync();
            await _appDbContext.SaveChangesAsync();
            return new OkResult();
        }

        [HttpPut]
        [Route("markassettled")]
        public async Task<IActionResult> MarkConsumptionsAsSettled(string codes)
        {
            var codesUint = codes.Split(',').Select(uint.Parse).ToList();
            foreach (var code in codesUint)
            {
                await _tconService.MarkCodeAsSettled((int)code);
            }
            await _appTallyDbContext.SaveChangesAsync();
            return new OkResult();
        }

        [HttpDelete]
        [Route("radios/offline/all")]
        public async Task<ObjectResult> DeleteAllCodesFromOfflineRadios()
        {
            var offlineRadios = (await _tconService.FindAllRadios()).Where(x => !x.IsOnline).ToList();
            if (offlineRadios.None()) throw new RadioException("Kan ikke fjerne kode fra radioer, da ingen radioer er offline.");

            var radioAccessCodes = await _tconService.FindAllRadioAccessCodes();
            var removedRadioCodes = new List<RadioAccessCode>();

            foreach (var radio in offlineRadios)
            {
                var radioAccessCodesForRadio = radioAccessCodes.Where(x => x.RadioAddress == radio.RadioAddress);
                foreach (var radioAccessCode in radioAccessCodesForRadio)
                {
                    await _tconService.RemoveRadioAccessCodeWithId(radioAccessCode.Id);
                    removedRadioCodes.Add(radioAccessCode);
                }
            }
            await _appTallyDbContext.SaveChangesAsync();
            var radioAccessCodeDtos = removedRadioCodes.Select(x => _mapper.Map<RadioAccessCodeDto>(x)).ToList();
            return new ObjectResult(radioAccessCodeDtos);
        }

        [HttpDelete]
        [Route("radios/all")]
        public async Task<ObjectResult> DeleteAllCodesFromAllRadios()
        {
            var offlineRadios = (await _tconService.FindAllRadios()).Where(x => !x.IsOnline).ToList();
            if (offlineRadios.None()) throw new RadioException("Kan ikke fjerne kode fra radioer, da ingen radioer er offline.");

            var radioAccessCodes = await _tconService.FindAllRadioAccessCodes();
            var removedRadioCodes = new List<RadioAccessCode>();

            foreach (var radio in offlineRadios)
            {
                var radioAccessCodesForRadio = radioAccessCodes.Where(x => x.RadioAddress == radio.RadioAddress);
                foreach (var radioAccessCode in radioAccessCodesForRadio)
                {
                    await _tconService.RemoveRadioAccessCodeWithId(radioAccessCode.Id);
                    removedRadioCodes.Add(radioAccessCode);
                }
            }
            await _appTallyDbContext.SaveChangesAsync();
            var radioAccessCodeDtos = removedRadioCodes.Select(x => _mapper.Map<RadioAccessCodeDto>(x)).ToList();
            return new ObjectResult(radioAccessCodeDtos);
        }

        [HttpDelete]
        [Route("radio")]
        public async Task<ObjectResult> DeleteAllCodesFromRadio(int radioAddress)
        {
            var radio = await _tconService.FindSingleRadio(radioAddress);

            var radioAccessCodes = await _tconService.FindAllRadioAccessCodes();
            var removedRadioCodes = new List<RadioAccessCode>();
            var radioAccessCodesForRadio = radioAccessCodes.Where(x => x.RadioAddress == radio.RadioAddress);
            foreach (var radioAccessCode in radioAccessCodesForRadio)
            {
                await _tconService.RemoveRadioAccessCodeWithId(radioAccessCode.Id);
                removedRadioCodes.Add(radioAccessCode);
            }
            await _appTallyDbContext.SaveChangesAsync();
            var radioAccessCodeDtos = removedRadioCodes.Select(x => _mapper.Map<RadioAccessCodeDto>(x)).ToList();
            return new ObjectResult(radioAccessCodeDtos);
        }

        private async Task<int> CreateAccessToGroups(RadioAccessCodeTenantDto radioCode)
        {
            var radioAccessCodeTenantDto = _mapper.Map<CreateRadioAccessFromAccessibleItemsDto>(radioCode);
            return await CreateAccessFromAccessibleItems(radioAccessCodeTenantDto);
        }

        private async Task<int> CreateAccessToAll(RadioAccessCodeTenantDto radioCode)
        {
            var tconService = GetTConService(radioCode.TenantId);
            var radioCodes =
                await tconService.AddAccessBasedOnRadiosFilter((int)radioCode.Code, radioCode.IsKeyCode, 0, new TallyRadiosFilter());
            await _appTallyDbContext.SaveChangesAsync();
            return radioCodes.Count;
        }

        private ITConService CreateTConServiceWithCustomClaimsProvider(Guid tenantId)
        {
            return _tconService.CloneWithCustomClaimsProvider(new TemporaryClaimsProvider(tenantId, AppConstants.SCHEDULER_ID, AppConstants.SCHEDULER_NAME, null, null));
        }

        private IMasterRadioIdProvider CreateMasterRadioIdProviderCustomClaimsProvider(Guid tenantId)
        {
            return _masterRadioIdProvider.CloneWithCustomClaimsProvider(new TemporaryClaimsProvider(tenantId, AppConstants.SCHEDULER_ID, AppConstants.SCHEDULER_NAME, null, null));
        }

        [HttpPost]
        [Route("deleteaccesscodesfromradiocodes/ifactivatedinlog")]
        public async Task<List<int>> DeleteRadioAccessesCodeFromRadioCodesIfActivatedInLog([FromBody] List<RadioAccessCodeTenantDto> radioAccessCodeTenantDtos)
        {
            var yesterDay = DateTime.Now.AddDays(-1).ToEuTimeZone();
            var cachedLogs = await TConRepository.FindTConTenantFilteredTBDAccessLogsExtended(_appTallyDbContext, yesterDay);
            var groupedRadioAccessCodes = radioAccessCodeTenantDtos.GroupBy(x => x.TenantId);
            int count = 0;
            List<int> deletedRadioCodes = new List<int>();

            foreach (var groupedRadioAccessCode in groupedRadioAccessCodes)
            {
                var masterRadioId = CreateMasterRadioIdProviderCustomClaimsProvider(groupedRadioAccessCode.Key).MasterRadioId;
                var filteredCachedLogs = cachedLogs.Where(x => x.MasterAddr == masterRadioId).ToList();
                var filteredRadioAccessCodes = groupedRadioAccessCode.Where(x => filteredCachedLogs.Any(cl => !cl.Rejected && cl.Code == x.Code && (x.ActivationDate == null || x.ActivationDate <= cl.SavedDateTime))).ToList();
                if (filteredRadioAccessCodes.Count > 0)
                {
                    await DeleteRadioAccessCodesFromRadioCodes(filteredRadioAccessCodes);
                    deletedRadioCodes = deletedRadioCodes.Concat(filteredRadioAccessCodes.Select(x => (int)x.Code)).ToList();
                }
            }

            return deletedRadioCodes;
        }

        [HttpPost]
        [Route("deleteaccesscodesfromradiocodes")]
        public async Task<List<int>> DeleteRadioAccessCodesFromRadioCodes([FromBody] List<RadioAccessCodeTenantDto> radioAccessCodes)
        {
            List<int> deletedRadioCodes = new List<int>();

            foreach (var radioCode in radioAccessCodes)
            {
                await CreateTConServiceWithCustomClaimsProvider(radioCode.TenantId).RemoveRadioAccessCodesFromAllRadiosWithCode((int)radioCode.Code);
                deletedRadioCodes.Add((int)radioCode.Code);
            }

            await _appTallyDbContext.SaveChangesAsync();
            return deletedRadioCodes;
        }

        [HttpPost]
        [Route("addaccesscodestoradiocodes")]
        public async Task<int> AddRadioAccessesCodeToRadioCodes([FromBody] List<RadioAccessCodeTenantDto> radioAccessCodeTenantDtos)
        {
            int codesCount = 0;
            foreach (var radioCode in radioAccessCodeTenantDtos)
            {
                if (radioCode.AccessibleItems == null)
                {
                    codesCount += await CreateAccessToAll(radioCode);
                }
                else
                {
                    codesCount += await CreateAccessToGroups(radioCode);
                }
            }

            return codesCount;
        }

    }
}
