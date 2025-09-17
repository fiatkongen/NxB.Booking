using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Munk.AspNetCore;
using Munk.Utils.Object;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NxB.Domain.Common.Interfaces;
using NxB.Dto;
using NxB.Dto.LoginApi;
using NxB.Dto.TenantApi;
using NxB.Settings.Shared.Infrastructure;
using NxB.BookingApi.Infrastructure;
using TenantModel = NxB.BookingApi.Models.Tenant;
using NxB.BookingApi.Models;

//http://hamidmosalla.com/2017/03/29/asp-net-core-action-results-explained/
//https://aspnetboilerplate.com/Pages/Documents/Validating-Data-Transfer-Objects
//https://www.devtrends.co.uk/blog/handling-errors-in-asp.net-core-web-api
namespace NxB.BookingApi.Controllers.Tenant
{
    [Produces("application/json")]
    [Authorize]
    [Route("tenant")]
    [ApiValidationFilter]
    public class TenantController : BaseController
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly TenantFactory _tenantFactory;
        private readonly AppDbContext _appDbContext;
        private readonly IUserClient _userClient;
        private readonly IClaimsProvider _claimsProvider;
        private readonly IMapper _mapper;
        private readonly TelemetryClient _telemetry;

        public TenantController(ITenantRepository tenantRepository, TenantFactory tenantFactory, AppDbContext appDbContext, IUserClient userClient, IClaimsProvider claimsProvider, ISettingsRepository settingsRepository, IMapper mapper, TelemetryClient telemetry)
        {
            _tenantRepository = tenantRepository;
            _tenantFactory = tenantFactory;
            _appDbContext = appDbContext;
            _userClient = userClient;
            _claimsProvider = claimsProvider;
            _mapper = mapper;
            _telemetry = telemetry;
        }

        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public ObjectResult FindSingleTenantPublic([NoEmpty]Guid id)
        {
            var tenant = _tenantRepository.FindSingleOrDefault(id);
            if (tenant == null) return new OkObjectResult(null);
            var tenantDto = _mapper.Map<TenantModel, TenantPublicDto>(tenant);
            return new OkObjectResult(tenantDto);
        }

        [HttpGet]
        [Route("subdomain")]
        [AllowAnonymous]
        public ObjectResult FindSingleTenantFromSubDomain([NoEmpty] string subDomain)
        {
            var tenant = _tenantRepository.FindSingleFromSubDomain(subDomain);
            if (tenant == null) return new BadRequestObjectResult("Kunne ikke finde subDomain: " + subDomain);

            var tenantDto = _mapper.Map<TenantModel, TenantPublicDto>(tenant);
            return new OkObjectResult(tenantDto);
        }


        [HttpGet]
        [Route("kioskid")]
        [AllowAnonymous]
        public ObjectResult FindSingleTenantFromKioskId([NoEmpty] string kioskId)
        {
            var tenant = _tenantRepository.FindSingleFromKioskId(kioskId);
            if (tenant == null) return new OkObjectResult(null);

            var tenantDto = _mapper.Map<TenantModel, TenantPublicDto>(tenant);
            return new OkObjectResult(tenantDto);
        }

        [HttpGet]
        [Route("private")]
        [AllowAnonymous]
        public ObjectResult FindSingleTenantPrivate([NoEmpty] Guid id)
        {
            var tenant = _tenantRepository.FindSingleOrDefault(id);
            if (tenant == null) return new BadRequestObjectResult("Kunne ikke finde id: " + id);

            var tenantDto = _mapper.Map<TenantModel, TenantDto>(tenant);
            return new OkObjectResult(tenantDto);
        }

        [HttpGet]
        [Route("current")]
        [AllowAnonymous]
        public ObjectResult FindCurrentTenant()
        {
            var tenant = _tenantRepository.FindSingleOrDefault(this._claimsProvider.GetTenantId());
            if (tenant == null) return new BadRequestObjectResult("Kunne ikke finde current tenant");

            var tenantDto = _mapper.Map<TenantModel, TenantPublicDto>(tenant);
            return new OkObjectResult(tenantDto);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("query/clientid")]
        public IActionResult FindSingleTenantFromClientId([Required] string id)
        {
            var tenant = _tenantRepository.FindSingleFromClientId(id);
            if (tenant == null) return new OkResult();

            var tenantDto = _mapper.Map<TenantModel, TenantPublicDto>(tenant);
            return new OkObjectResult(tenantDto);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("tenant")]
        public IActionResult FindSingleTenantFromTenantId([Required] string tenantId)
        {
            var tenant = _tenantRepository.FindSingleFromLegacyId(tenantId);
            if (tenant == null) return new OkResult();

            var tenantDto = _mapper.Map<TenantModel, TenantPublicDto>(tenant);
            return new OkObjectResult(tenantDto);
        }


        [HttpGet]
        [AllowAnonymous]
        [Route("query/legacyid")]
        public IActionResult FindSingleTenantFromLegacyId([Required] string id)
        {
            var tenant = _tenantRepository.FindSingleFromLegacyId(id);
            if (tenant == null) return new OkResult();

            var tenantDto = _mapper.Map<TenantModel, TenantDto>(tenant);
            return new OkObjectResult(tenantDto);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("loggedin")]
        public IActionResult FindLoggedInTenant()
        {
            var clientId = _claimsProvider.GetTenantId();
            var tenant = _tenantRepository.FindSingle(clientId);
            if (tenant == null) return new OkResult();

            var tenantDto = _mapper.Map<TenantModel, TenantPublicDto>(tenant);
            return new OkObjectResult(tenantDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("")]
        public async Task<ObjectResult> CreateTenant([FromBody] CreateTenantDto tenantDto)
        {
            var tenant = tenantDto.SubDomain != null ? _tenantRepository.FindSingleFromSubDomain(tenantDto.SubDomain) : null;
            if (tenant != null) throw new Exception($"tenant subdomain: {tenantDto.SubDomain}, is already takeb");

            tenant = _tenantRepository.FindSingleFromClientId(tenantDto.ClientId);
            if (tenant != null) throw new Exception($"tenant clientId: {tenantDto.ClientId}, is already taken");

            TenantModel newTenant = _tenantFactory.Create(tenantDto.ClientId);
            _mapper.Map(tenantDto, newTenant);

            _tenantRepository.Add(newTenant);
            await _appDbContext.SaveChangesAsync();

            try
            {
                await _userClient.AddTenantToAdministrator(newTenant.Id);
                await _userClient.AddTenantToOnlineUser(newTenant.Id);
            }
            catch (Exception exception)
            {
                _telemetry.TrackException(exception);
            }

            return new CreatedResult(new Uri("?id=" + newTenant.Id, UriKind.Relative), newTenant);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        [Route("")]
        public IActionResult ModifyTenant([FromBody] TenantDto tenantDto)
        {
            var tenant = _tenantRepository.FindSingleOrDefault(tenantDto.Id);
            if (tenant == null) return new NotFoundObjectResult(tenantDto.Id);

            _mapper.Map(tenantDto, tenant);
            _tenantRepository.Update(tenant);
            _appDbContext.SaveChanges();

            return new OkResult();
        }

        [HttpPut]
        [Route("public")]
        public IActionResult ModifyTenantPublic([FromBody] ModifyTenantPublicDto dto)
        {
            var tenant = _tenantRepository.FindSingleOrDefault(dto.Id);
            if (tenant == null) return new NotFoundObjectResult(dto.Id);

            _mapper.Map(dto, tenant);
            _tenantRepository.Update(tenant);
            _appDbContext.SaveChanges();

            return new OkResult();
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        [Route("")]
        public IActionResult DeleteTenant([NoEmpty]Guid id)
        {
            var tenant = _tenantRepository.FindSingleOrDefault(id);
            if (tenant == null) return new NotFoundObjectResult(id);
            tenant.IsDeleted = true;
            _appDbContext.SaveChanges();

            return new OkResult();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("list/all/private")]
        public async Task<ObjectResult> FindAllPrivateTenants()
        {
            var tenants = await _tenantRepository.FindAll();
            var tenantDtos = _mapper.Map<IEnumerable<TenantDto>>(tenants);
            return new OkObjectResult(tenantDtos);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("list/all")]
        public async Task<ObjectResult> FindAllPublicTenants()
        {
            var tenants = await _tenantRepository.FindAll();
            var tenantDtos = _mapper.Map<IEnumerable<TenantPublicDto>>(tenants);
            return new OkObjectResult(tenantDtos);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("list/all/active")]
        public async Task<ObjectResult> FindAllActiveTenants()
        {
            var tenants = await _tenantRepository.FindAllActive();
            var tenantDtos = _mapper.Map<IEnumerable<TenantPublicDto>>(tenants);
            return new OkObjectResult(tenantDtos);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("list/all/ids")]
        public async Task<ObjectResult> FindAllTenantIds()
        {
            var tenants = await _tenantRepository.FindAllActive();
            var tenantDictionary = _mapper.Map<IEnumerable<TenantDto>>(tenants).OrderBy(x => x.ClientId).ToDictionary(x => x.ClientId, x => x.CompanyName);
            return new OkObjectResult(tenantDictionary);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("delete/bookings")]
        public async Task<IActionResult> DeleteBookings([FromQuery] Guid tenantId, bool deleteImported)
        {
            await _tenantRepository.DeleteBookings(tenantId, deleteImported);
            return new OkResult();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("delete/customers")]
        public IActionResult DeleteCustomers([FromQuery] Guid tenantId)
        {
            _tenantRepository.DeleteCustomers(tenantId);
            return new OkResult();
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        [Route("delete/booking/imported")]
        public async Task<IActionResult> DeleteBooking(string bookingId)
        {
            await _tenantRepository.DeleteSingleBooking(_claimsProvider.GetTenantId(), bookingId);
            return new OkResult();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("delete/invoices")]
        public IActionResult DeleteVouchers([FromQuery] Guid tenantId)
        {
            _tenantRepository.DeleteVouchers(tenantId);
            return new OkResult();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("delete/messages")]
        public IActionResult DeleteMessages([FromQuery] Guid tenantId)
        {
            _tenantRepository.DeleteMessages(tenantId);
            return new OkResult();
        }

    }
}