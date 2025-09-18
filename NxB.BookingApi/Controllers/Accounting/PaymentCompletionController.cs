using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Munk.AspNetCore;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;
using NxB.Dto.AccountingApi;
using NxB.Clients.Interfaces;

namespace NxB.BookingApi.Controllers.Accounting
{
    [Produces("application/json")]
    [Route("paymentcompletion")]
    [Authorize]
    [ApiValidationFilter]
    public class PaymentCompletionController : BaseController
    {
        private readonly IPaymentCompletionRepository _paymentCompletionRepository;
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly TelemetryClient _telemetryClient;
        private readonly ICounterPushUpdateService _counterPushUpdateService;

        public PaymentCompletionController(IPaymentCompletionRepository paymentCompletionRepository, AppDbContext appDbContext, IMapper mapper, TelemetryClient telemetryClient, ICounterPushUpdateService counterPushUpdateService)
        {
            _paymentCompletionRepository = paymentCompletionRepository;
            _appDbContext = appDbContext;
            _mapper = mapper;
            _telemetryClient = telemetryClient;
            _counterPushUpdateService = counterPushUpdateService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("")]
        public async Task<IActionResult> CreatePaymentCompletion(CreatePaymentCompletionDto createDto)
        {
            var paymentCompletion = new PaymentCompletion(
                Guid.NewGuid(),
                createDto.TenantId,
                createDto.Amount,
                createDto.VoucherType,
                createDto.FriendlyOrderId,
                createDto.FriendlyPaymentId,
                createDto.FriendlyVoucherId,
                createDto.QuickPayPaymentId,
                createDto.QuickPayOrderId,
                createDto.Success,
                createDto.ResponseJson,
                createDto.InitiatedByUser,
                createDto.IsFeeAdded,
                createDto.IsAutoCaptured,
                createDto.PaymentLinkId,
                createDto.State,
                createDto.TransactionType,
                createDto.IsLegacy,
                createDto.IsPending,
                createDto.LinkSource);

            await _paymentCompletionRepository.Add(paymentCompletion);
            await _appDbContext.SaveChangesAsync();

            var paymentCompletionDto = _mapper.Map<PaymentCompletionDto>(paymentCompletion);

            return new CreatedResult(new Uri("?id=" + paymentCompletionDto.Id, UriKind.Relative), paymentCompletionDto);
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindPaymentCompletions(DateTime? start = null, DateTime? end = null, bool includeArchived = true)
        {
            start ??= DateTime.Today.AddMonths(-1);
            end ??= DateTime.Today.AddDays(1);
            var paymentCompletions = await _paymentCompletionRepository.FindAll(new DateInterval(start.Value, end.Value.Date.AddDays(1)), includeArchived);
            var paymentCompletionsDtos = paymentCompletions.Select(x => _mapper.Map<PaymentCompletionDto>(x)).ToList();
            return new OkObjectResult(paymentCompletionsDtos);
        }

        [HttpGet]
        [Route("count/active")]
        public async Task<int> CountActivePaymentCompletions(Guid? tenantId = null)
        {
            var paymentCompletionRepository = _paymentCompletionRepository;
            if (tenantId != null)
            {
                paymentCompletionRepository = _paymentCompletionRepository.CloneWithCustomClaimsProvider(TemporaryClaimsProvider.CreateAdministrator(tenantId.Value));
            }
            var count = await paymentCompletionRepository.CountActive();
            return count;
        }

        public async Task TryBroadcastActiveTransactions()
        {
            var count = await _paymentCompletionRepository.CountActive();
            await _counterPushUpdateService.TryPushActiveTransactionsCounter(count);
        }

        [HttpPut]
        [Route("archive")]
        public async Task<IActionResult> ArchivePaymentCompletion(Guid id)
        {
            var paymentCompletion = await _paymentCompletionRepository.FindSingle(id);
            paymentCompletion.IsArchived = true;
            _paymentCompletionRepository.Update(paymentCompletion);
            await _appDbContext.SaveChangesAsync();
            await TryBroadcastActiveTransactions();
            return new OkResult();
        }

        [HttpPut]
        [Route("unarchive")]
        public async Task<IActionResult> UnArchivePaymentCompletion(Guid id)
        {
            var paymentCompletion = await _paymentCompletionRepository.FindSingle(id);
            paymentCompletion.IsArchived = false;
            _paymentCompletionRepository.Update(paymentCompletion);
            await _appDbContext.SaveChangesAsync();
            await TryBroadcastActiveTransactions();
            return new OkResult();
        }

        [HttpPut]
        [Route("archive/all/completed")]
        public async Task<IActionResult> ArchiveAllCompleted()
        {
            await _paymentCompletionRepository.ArchiveAllCompleted();
            await _appDbContext.SaveChangesAsync();
            await TryBroadcastActiveTransactions();
            return new OkResult();
        }

        [HttpPut]
        [Route("pending/remove")]
        public async Task<IActionResult> RemovePending(long friendlyOrderId)
        {
            await _paymentCompletionRepository.RemovePending(friendlyOrderId);
            await _appDbContext.SaveChangesAsync();
            return new OkResult();
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        [Route("")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var paymentCompletion = await _paymentCompletionRepository.FindSingle(id);
            _paymentCompletionRepository.Delete(paymentCompletion);
            await _appDbContext.SaveChangesAsync();
            await TryBroadcastActiveTransactions();
            return new OkResult();
        }
    }
}
