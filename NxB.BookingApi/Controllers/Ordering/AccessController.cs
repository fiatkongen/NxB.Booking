using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using Munk.Utils.Object;
using Newtonsoft.Json;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.Clients;
using NxB.Dto.OrderingApi;
using NxB.Dto.TallyWebIntegrationApi;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;
using NxB.Settings.Shared.Infrastructure;


namespace NxB.BookingApi.Controllers.Ordering
{
    [Produces("application/json")]
    [Route("access")]
    [Authorize]
    [ApiValidationFilter]
    public class AccessController : BaseController
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly AccessFactory _accessFactory;
        private readonly IAccessRepository _accessRepository;
        private readonly IRadioAccessCodeClient _radioAccessCodeClient;
        private readonly IOrderRepository _orderRepository;
        private readonly TelemetryClient _telemetry;
        private readonly ISettingsRepository _settingsRepository;
        private readonly IKeyCodeGenerator _keyCodeGenerator;
        private readonly IClaimsProvider _claimsProvider;

        public AccessController(AppDbContext appDbContext, IMapper mapper, AccessFactory accessFactory, IAccessRepository accessRepository, IRadioAccessCodeClient radioAccessCodeClient, IOrderRepository orderRepository, TelemetryClient telemetry, ISettingsRepository settingsRepository, IKeyCodeGenerator keyCodeGenerator, IClaimsProvider claimsProvider)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _accessFactory = accessFactory;
            _accessRepository = accessRepository;
            _radioAccessCodeClient = radioAccessCodeClient;
            _orderRepository = orderRepository;
            _telemetry = telemetry;
            _settingsRepository = settingsRepository;
            _keyCodeGenerator = keyCodeGenerator;
            _claimsProvider = claimsProvider;
        }

        [HttpGet]
        [Route("")]
        public async Task<ObjectResult> FindAccess([NoEmpty] Guid id)
        {
            var access = await _accessRepository.FindSingleOrDefaultAccess(id);
            if (access == null) return null;
            var accessDto = _mapper.Map<AccessDto>(access);
            return new ObjectResult(accessDto);
        }

        [HttpGet]
        [Route("keycode/generate/next")]
        public async Task<ObjectResult> GenerateNextAvailableCode()
        {
            var nextCode = await _keyCodeGenerator.Next(_claimsProvider.ToDto());
            var dictionary = new Dictionary<string, int> { { "result", nextCode } };
            return new ObjectResult(dictionary);
        }

        [HttpPost]
        [Route("")]
        public async Task<ObjectResult> CreateAccess([FromBody] CreateAccessDto createAccessDto)
        {
            if (createAccessDto.IsKeyCode && createAccessDto.Code != null)
                throw new AccessException("Cannot create access with keycode, when keycode != null and isKeyCode = true");

            Access access = null;
            try
            {
                access = await CreateKeyCardCodeAccess(createAccessDto);
                await ModifyAccessAndHandleActivation(access, async () =>
                {
                    var createRadioAccessDto = new CreateRadioAccessDto
                    {
                        IsKeyCode = access.IsKeyCode,
                        Code = (uint)access.Code,
                        SocketsFilter = createAccessDto.SocketsFilter,
                        SwitchesFilter = createAccessDto.SwitchesFilter,
                        SocketRadios = createAccessDto.SocketRadios,
                        SwitchRadios = createAccessDto.SwitchRadios,
                        Option = 0
                    };
                    await _radioAccessCodeClient.CreateRadioAccessCode(createRadioAccessDto);
                });
                await _appDbContext.SaveChangesAsync();
            }
            catch (ServiceStack.WebServiceException exception)
            {
                throw new AccessException($"Kan ikke udlåne kort/kode: {access.Code}. " + exception.ResponseBody, exception);
            }

            var accessDto = _mapper.Map<AccessDto>(access);
            return new CreatedResult(new Uri("?id=" + access.Id, UriKind.Relative), accessDto);
        }

        private async Task ModifyAccessAndHandleActivation(Access access, Func<Task> onAccessActivation)
        {
            if (_settingsRepository.GetIsTallyAutoActivationEnabled())
            {
                //if (access.AutoActivationDate != null && access.AutoActivationDate <= DateTime.Now.ToEuTimeZone())
                //{
                //    access.Activate(); //Execution should not be delayed
                //}

                if (access.AccessType == AccessType.OneOff)
                {
                    access.AutoActivationDate = null;
                    if (access.AutoDeactivationDate == null)
                    {
                        access.AutoDeactivationDate = DateTime.Now.AddDays(1).ToEuTimeZone();
                    }
                }

                if (access.AutoDeactivationDate != null && access.AutoDeactivationDate <= DateTime.Now.ToEuTimeZone())
                {
                    throw new AccessException("Kan ikke aktivere kort/kode med en slutdato inden dags dato");
                }

                if (access.AutoActivationDate != null &&
                    access.AutoActivationDate > DateTime.Now.ToEuTimeZone()) return;
            }

            access.Activate();
            await onAccessActivation();
        }

        [HttpPost]
        [Route("accessibleitems")]
        public async Task<ObjectResult> CreateAccessToAccessibleItems([FromBody] CreateOrModifyAccessFromAccessibleItemsDto createDto)
        {
            if (createDto.IsKeyCode && createDto.Code != null)
                throw new AccessException("Cannot create access with keycode, when keycode != null and isKeyCode = true");

            Access access = null;
            try
            {
                access = await CreateKeyCardCodeAccess(createDto);
                access.AccessibleItems = createDto.AccessibleItems;

                await ModifyAccessAndHandleActivation(access, async () =>
                {
                    await _radioAccessCodeClient.CreateRadioAccessCodesFromAccessibleItems(
                        new CreateOrModifyAccessFromAccessibleItemsDto
                        {
                            IsKeyCode = access.IsKeyCode,
                            Code = (uint)access.Code,
                            AccessibleItems = createDto.AccessibleItems
                        });
                });
                await _appDbContext.SaveChangesAsync();
            }
            catch (ServiceStack.WebServiceException exception)
            {
                throw new AccessException($"Kan ikke udlåne kort/kode: {access.Code}. " + exception.ResponseBody, exception);
            }

            var accessDto = _mapper.Map<AccessDto>(access);
            return new CreatedResult(new Uri("?id=" + access.Id, UriKind.Relative), accessDto);
        }

        [HttpPut]
        [Route("accessibleitems")]
        public async Task<ObjectResult> ModifyAccessToAccessibleItems([FromBody] ModifyAccessFromAccessibleItemsDto modifyDto)
        {
            Access access = await _accessRepository.FindAccess(modifyDto.Id);
            try
            {
                access.AccessibleItems = modifyDto.AccessibleItems;
                await _radioAccessCodeClient.ModifyRadioAccessCodesFromAccessibleItems(new CreateOrModifyAccessFromAccessibleItemsDto
                {
                    IsKeyCode = access.IsKeyCode,
                    Code = (uint)access.Code,
                    AccessibleItems = modifyDto.AccessibleItems
                });
                await _appDbContext.SaveChangesAsync();
            }
            catch (ServiceStack.WebServiceException exception)
            {
                throw new AccessException($"Kan ikke opdatere adgange for kort/kode: {access.Code}. " + exception.ResponseBody, exception);
            }

            var accessDto = _mapper.Map<AccessDto>(access);
            return new OkObjectResult(accessDto);
        }

        private async Task<Access> CreateKeyCardCodeAccess(IAccessDto createAccessDto)
        {
            Access access;

            if (!createAccessDto.IsKeyCode)
            {
                access = await _accessFactory.CreateCardCodeAccess((int)createAccessDto.Code.Value);
                _mapper.Map(createAccessDto, access);
            }
            else
            {
                access = await _accessFactory.CreateKeyCodeAccess();
                createAccessDto.Code = (uint)access.Code;
                _mapper.Map(createAccessDto, access);
            }

            var existingAccess = await _accessRepository.FindActiveAccessFromCode((int)createAccessDto.Code);
            if (existingAccess != null)
            {
                var order = await _orderRepository.FindSingleFromSubOrderId(existingAccess.SubOrderId, false);
                throw new AccessException(
                    $"Kan ikke udlåne kort/kode {createAccessDto.Code}. Kortet/koden er allerede udlånt til booking {order.FriendlyId.DefaultIdPadding()}");
            }

            _accessRepository.Add(access);
            return access;
        }

        [HttpPut]
        [Route("markasdeleted")]
        public async Task<ObjectResult> MarkAccessAsDeleted(Guid id)
        {
            var access = await _accessRepository.MarkAsDeleted(id);
            await _appDbContext.SaveChangesAsync();
            var accessDto = _mapper.Map<AccessDto>(access);
            return new OkObjectResult(accessDto);
        }

        [HttpPut]
        [Route("returncode")]
        public async Task<ObjectResult> ReturnCode(uint code, bool markAsSettled = true)
        {
            var intCode = (int)code;
            var access = await _accessRepository.DeactivateFromCode(intCode);
            try
            {
                await _radioAccessCodeClient.RemoveAccessFromCode((uint)intCode, markAsSettled);
            }
            catch (Exception exception)
            {
                throw new AccessException($"Kan ikke slette adgang til kort/kode: {code}. " + exception.Message);
            }
            await _appDbContext.SaveChangesAsync();

            AccessDto accessDto = null;
            if (access != null)
            {
                accessDto = _mapper.Map<AccessDto>(access);
            }
            return new OkObjectResult(accessDto);
        }

        [HttpPut]
        [Route("deactivate")]
        public async Task<ObjectResult> DeactivateAccess(Guid id, bool markAsSettled = true)
        {
            var access = await _accessRepository.Deactivate(id);
            try
            {
                await _radioAccessCodeClient.RemoveAccessFromCode((uint)access.Code, markAsSettled);
            }
            catch (Exception exception)
            {
                throw new AccessException($"Kan ikke slette adgang til kort/kode: {(uint)access.Code}. " + exception.Message, exception);
            }
            await _appDbContext.SaveChangesAsync();
            var accessDto = _mapper.Map<AccessDto>(access);
            return new OkObjectResult(accessDto);
        }

        [HttpPut]
        [Route("reservations/ended/deactivate")]
        public async Task<ObjectResult> DeActivateAccessForEndedReservations()
        {
            var dateInterval = new DateInterval(DateInterval.Eternal.Start, DateTime.Today.AddDays(-1));
            var activeAccesses = await _accessRepository.FindAllActive();
            //activeAccesses = activeAccesses.Where(x => x.AccessNames.Contains("aster")).ToList();

            var uniqueSubOrderIds = activeAccesses.OrderByDescending(x => x.CreateDate).Select(x => x.SubOrderId).Distinct();
            var endedAccesses = new List<Access>();
            var batchSize = 100;
            var batSizeReached = false;

            try
            {
                foreach (var subOrderId in uniqueSubOrderIds)
                {
                    var order = await _orderRepository.FindSingleFromSubOrderId(subOrderId, false);
                    if (order == null) continue;
                    var endedSubOrderValidForDeactivation = order.SubOrders.SingleOrDefault(su => su.Start >= dateInterval.Start && su.End <= dateInterval.End && su.Id == subOrderId);

                    if (endedSubOrderValidForDeactivation != null)
                    {
                        var accessesToBeDeactivated = activeAccesses.Where(x => x.SubOrderId == subOrderId);
                        foreach (var access in accessesToBeDeactivated)
                        {
                            access.Deactivate();
                            await _radioAccessCodeClient.RemoveAccessFromCode((uint)access.Code, true);
                            await _appDbContext.SaveChangesAsync();
                            endedAccesses.Add(access);
                            batSizeReached = batchSize == endedAccesses.Count;
                            if (batSizeReached) break;
                        }
                    }
                    if (batSizeReached) break;
                }
            }
            catch (Exception exception)
            {
                throw new AccessException($"Kan ikke deaktivere adgange " + exception.Message, exception);
            }
            var accessDtos = endedAccesses.Select(x => _mapper.Map<AccessDto>(x));
            return new OkObjectResult(accessDtos);
        }

        [HttpPut]
        [Route("reservations/ended/reactivate")] //for emergencies
        public async Task<ObjectResult> ReActivateAccessForEndedReservations()
        {
            var activeAccesses = await _accessRepository.FindAllActive();
            //activeAccesses = activeAccesses.Where(x => x.AccessNames.Contains("aster")).ToList();
            var endedAccesses = new List<Access>();

            try
            {
                foreach (var access in activeAccesses)
                {
                    access.Reactivate();
                    try
                    {
                        await _radioAccessCodeClient.CreateRadioAccessCode(new CreateRadioAccessDto
                        {
                            Code = (uint)access.Code,
                            IsKeyCode = access.IsKeyCode,
                            Option = 10,
                            SwitchesFilter = RadioUnitsFilter.All
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("could not reactive access " + ex.Message);
                    }

                    await _appDbContext.SaveChangesAsync();
                    endedAccesses.Add(access);
                }
            }
            catch (Exception exception)
            {
                throw new AccessException($"Kan ikke genaktivere adgange " + exception.Message, exception);
            }
            var accessDtos = endedAccesses.Select(x => _mapper.Map<AccessDto>(x));
            return new OkObjectResult(accessDtos);
        }

        [HttpPut]
        [Route("reactivate")]
        public async Task<ObjectResult> ReActivateAccess(Guid id)
        {
            var access = await _accessRepository.FindAccess(id);
            if (access.IsKeyCode)
            {
                throw new AccessException("Kan ikke genaktivere en kode. Kan kun genaktivere et kort");
            }
            access = await _accessRepository.Reactivate(id);
            try
            {
                await _radioAccessCodeClient.CreateRadioAccessCode(new CreateRadioAccessDto
                {
                    Code = (uint)access.Code,
                    IsKeyCode = false
                });
            }
            catch (Exception exception)
            {
                throw new AccessException($"Kan ikke genaktivere kort: {(uint)access.Code}. " + exception.Message, exception);
            }
            await _appDbContext.SaveChangesAsync();
            var accessDto = _mapper.Map<AccessDto>(access);
            return new OkObjectResult(accessDto);
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAllAccesses()
        {
            var accesses = await _accessRepository.FindAllActive();
            var accessDtos = accesses.Select(x => _mapper.Map<AccessDto>(x)).ToList();
            return new OkObjectResult(accessDtos);
        }

        [HttpGet]
        [Route("list/all/active")]
        public async Task<ObjectResult> FindAllActiveAccesses()
        {
            var accesses = await _accessRepository.FindAllActive();
            var accessDtos = accesses.Select(x => _mapper.Map<AccessDto>(x)).ToList();
            return new OkObjectResult(accessDtos);
        }

        [HttpGet]
        [Route("suborder")]
        public async Task<ObjectResult> FindAccessesFromSubOrderId(Guid subOrderId)
        {
            var accesses = await _accessRepository.FindFromSubOrderId(subOrderId);
            var accessDtos = accesses.Select(x => _mapper.Map<AccessDto>(x)).ToList();
            return new OkObjectResult(accessDtos);
        }

        [HttpGet]
        [Route("order/list")]
        public async Task<ObjectResult> FindAccessesFromOrderId(Guid orderId, bool? isKeyCode = null)
        {
            var order = await _orderRepository.FindSingle(orderId, false);
            List<AccessDto> accessDtos = new List<AccessDto>();

            foreach (var subOrder in order.SubOrdersNotEqualized())
            {
                var accesses = await _accessRepository.FindFromSubOrderId(subOrder.Id, isKeyCode);
                accessDtos = accessDtos.Concat(accesses.Select(x => _mapper.Map<AccessDto>(x)).ToList()).ToList();
            }
            return new OkObjectResult(accessDtos);
        }

        [HttpGet]
        [Route("code")]
        public async Task<ObjectResult> FindActiveAccessesFromCode(uint code)
        {
            AccessDto accessDto = null;
            var access = await _accessRepository.FindActiveAccessFromCode((int)code);
            if (access != null)
            {
                accessDto = _mapper.Map<AccessDto>(access);
            }
            return new OkObjectResult(accessDto);
        }
    }
}
