using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using NxB.BookingApi.Models;
using NxB.BookingApi.Infrastructure;
using NxB.Settings.Shared.Infrastructure;

namespace NxB.BookingApi.Controllers.Pricing
{
    [Produces("application/json")]
    [Route("paymentprovider")]
    [Authorize]
    [ApiValidationFilter]
    public class PaymentProviderController : BaseController
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly ISettingsRepository _settingsRepository;

        public PaymentProviderController(AppDbContext appDbContext, IMapper mapper, ISettingsRepository settingsRepository)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _settingsRepository = settingsRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("list/all/tenant")]
        public async Task<ObjectResult> FindPaymentProfilesFromTenantId(Guid tenantId)
        {
            var paymentProviders = _settingsRepository.GetOnlineBookingPaymentProviders(tenantId);
            return new OkObjectResult(paymentProviders);
        }
    }
}