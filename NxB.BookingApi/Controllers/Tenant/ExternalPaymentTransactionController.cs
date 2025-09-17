using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;
using NxB.Dto.TenantApi;
using Microsoft.AspNetCore.Authorization;
using Munk.AspNetCore;

namespace NxB.BookingApi.Controllers.Tenant
{
    [Produces("application/json")]
    [Route("externalpaymenttransaction")]
    [Authorize]
    [ApiValidationFilter]
    public class ExternalPaymentTransactionController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly TelemetryClient _telemetry;
        private readonly AppDbContext _appDbContext;
        private readonly IExternalPaymentTransactionRepository _externalPaymentTransactionRepository;

        public ExternalPaymentTransactionController(IMapper mapper, TelemetryClient telemetry, AppDbContext appDbContext, IExternalPaymentTransactionRepository externalPaymentTransactionRepository)
        {
            _mapper = mapper;
            _telemetry = telemetry;
            _appDbContext = appDbContext;
            _externalPaymentTransactionRepository = externalPaymentTransactionRepository;
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAllTransactions()
        {
            var externalPaymentTransactions = await _externalPaymentTransactionRepository.FindAll();
            var dtos = externalPaymentTransactions.Select(x => _mapper.Map<ExternalPaymentTransactionDto>(x));
            return new ObjectResult(dtos);
        }

        [HttpGet]
        [Route("list/all/voucherid")]
        public async Task<ObjectResult> FindAllTransactionsFromVoucherId(Guid voucherId)
        {
            var externalPaymentTransactions = await _externalPaymentTransactionRepository.FindAllFromVoucherId(voucherId);
            var dtos = externalPaymentTransactions.Select(x => _mapper.Map<ExternalPaymentTransactionDto>(x));
            return new ObjectResult(dtos);
        }
    }
}