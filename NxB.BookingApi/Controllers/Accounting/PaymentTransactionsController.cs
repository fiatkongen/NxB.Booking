using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using Newtonsoft.Json;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Constants;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;
using NxB.Dto.AccountingApi;
using NxB.Clients.Interfaces;
using NxB.Settings.Shared.Infrastructure;
using QuickPay.SDK;

namespace NxB.BookingApi.Controllers.Accounting
{
    [Produces("application/json")]
    [Route("paymenttransaction")]
    [Authorize]
    [ApiValidationFilter]
    public class PaymentTransactionsController : BaseController
    {
        private readonly IPaymentCompletionRepository _paymentCompletionRepository;
        private readonly AppDbContext _appDbContext;
        private readonly TelemetryClient _telemetryClient;
        private readonly ISettingsRepository _settingsRepository;
        private readonly ITenantClient _tenantClient;
        private readonly IMapper _mapper;
        private readonly ICounterPushUpdateService _counterPushUpdateService;

        public PaymentTransactionsController(
            IPaymentCompletionRepository paymentCompletionRepository,
            AppDbContext appDbContext,
            TelemetryClient telemetryClient,
            ISettingsRepository settingsRepository,
            ITenantClient tenantClient,
            IMapper mapper,
            ICounterPushUpdateService counterPushUpdateService)
        {
            _paymentCompletionRepository = paymentCompletionRepository;
            _appDbContext = appDbContext;
            _telemetryClient = telemetryClient;
            _settingsRepository = settingsRepository;
            _tenantClient = tenantClient;
            _mapper = mapper;
            _counterPushUpdateService = counterPushUpdateService;
        }

        [HttpPost]
        [Route("capture")]
        public async Task<IActionResult> Capture(Guid paymentCompletionId)
        {
            var paymentCompletion = await _paymentCompletionRepository.FindSingle(paymentCompletionId);
            var quickPayPaymentId = paymentCompletion.QuickPayPaymentId;
            var amount = paymentCompletion.Amount;

            var quickPayClient = CreateQuickPayClient();
            var capture = await quickPayClient.Payments.Capture(quickPayPaymentId, Convert.ToInt32(amount * 100)).ConfigureAwait(false);

            if (!capture.Accepted)
            {
                _telemetryClient.TrackTrace("QuickPay capture error: captureJson: " + JsonConvert.SerializeObject(capture));
                throw new PaymentCaptureException("Fejl ved capture");
            }
            await _appDbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        [Route("cancel")]
        public async Task<IActionResult> Cancel(Guid paymentCompletionId)
        {
            var paymentCompletion = await _paymentCompletionRepository.FindSingle(paymentCompletionId);
            var quickPayPaymentId = paymentCompletion.QuickPayPaymentId;

            var quickPayClient = CreateQuickPayClient();
            var result = await quickPayClient.Payments.Cancel(quickPayPaymentId).ConfigureAwait(false);

            if (!result.Accepted)
            {
                _telemetryClient.TrackTrace("QuickPay cancel error: cancel json: " + JsonConvert.SerializeObject(result));
                throw new PaymentCaptureException("Fejl ved annullér");
            }

            await TryBroadcastActiveTransactions();
            return Ok();
        }

        [HttpPost]
        [Route("refund")]
        public async Task<IActionResult> Refund(Guid paymentCompletionId, decimal? amount, bool addPayment = false)
        {
            var paymentCompletion = await _paymentCompletionRepository.FindSingle(paymentCompletionId);
            var quickPayPaymentId = paymentCompletion.QuickPayPaymentId;
            amount ??= paymentCompletion.Amount;

            paymentCompletion.OnCompletionAction = addPayment ? PaymentCompletionAction.AppendPayment  : PaymentCompletionAction.None;
            _paymentCompletionRepository.Update(paymentCompletion);
            await _appDbContext.SaveChangesAsync();

            var quickPayClient = CreateQuickPayClient();
            var result = await quickPayClient.Payments.Refund(quickPayPaymentId, Convert.ToInt32(amount * 100)).ConfigureAwait(false);

            if (!result.Accepted)
            {
                _telemetryClient.TrackTrace("QuickPay refund error: captureJson: " + JsonConvert.SerializeObject(result));
                throw new PaymentCaptureException("Fejl ved refund. Check om du har tilbage betalt mere end det oprindelig");
            }

            if (addPayment)
            {
                await TryBroadcastActiveTransactions();
            }
            return Ok();
        }

        [HttpPost]
        [Route("refund/payment")]
        public async Task<IActionResult> RefundPayment(Guid paymentId, decimal? amount, bool addPayment = false)
        {
            var paymentCompletion = await _paymentCompletionRepository.FindSingleOrDefaultFromPaymentId(paymentId);
            return await Refund(paymentCompletion.Id, amount, addPayment);
        }

        [HttpPost]
        [Route("refund/paymentcompletion")]
        public async Task<IActionResult> RefundPaymentCompletion(Guid paymentCompletionId, decimal? amount, bool addPayment = false)
        {
            var paymentCompletion = await _paymentCompletionRepository.FindSingle(paymentCompletionId);
            return await Refund(paymentCompletion.Id, amount, addPayment);
        }

        public QuickPayClient CreateQuickPayClient(Guid? tenantId = null)
        {
            var quickPaySettings = _settingsRepository.GetQuickPaySettings(tenantId);
            return new QuickPayClient(quickPaySettings.QuickPayApiUser, quickPaySettings.QuickPayPrivateKey, quickPaySettings.QuickPayUserKey);
        }

        [HttpGet]
        [Route("payment/isrefundable")]
        public async Task<IActionResult> IsPaymentRefundable(Guid paymentId)
        {
            var paymentCompletion = await _paymentCompletionRepository.FindSingleOrDefaultFromPaymentId(paymentId);
            if (paymentCompletion == null)
            {
                return ReturnFalse();
            }

            if (!paymentCompletion.IsLegacy && paymentCompletion.FriendlyPaymentId != null)
            {
                var existingRefund = await _paymentCompletionRepository.FindSingleOrDefaultFromFriendlyVoucherId(paymentCompletion.FriendlyPaymentId.Value, paymentCompletion.VoucherType.Value, "refund");
                if (existingRefund != null)
                {
                    return ReturnFalse();
                }
            }
            else if (!paymentCompletion.IsLegacy && paymentCompletion.FriendlyPaymentId == null) //onlinebooking
            {
                var existingRefund = await _paymentCompletionRepository.FindSingleOrDefaultFromFriendlyOrderId(paymentCompletion.FriendlyOrderId, "refund");
                if (existingRefund != null)
                {
                    return ReturnFalse();
                }
            }
            else if (paymentCompletion.IsLegacy)
            {
                var existingRefund = await _paymentCompletionRepository.FindSingleOrDefaultFromFriendlyOrderId(paymentCompletion.FriendlyOrderId, "refund");
                if (existingRefund != null)
                {
                    return ReturnFalse();
                }
            }

            return ReturnTrue();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("payment/completed/quickpayid")]
        public async Task<ObjectResult> FindCompletedPayment(int quickPayPaymentId)
        {
            var paymentCompletion = await _paymentCompletionRepository.FindSingleOrDefaultFromQuickPayPaymentId(quickPayPaymentId, "authorize");
            if (paymentCompletion == null)
            {
                paymentCompletion = await _paymentCompletionRepository.FindSingleOrDefaultFromQuickPayPaymentId(quickPayPaymentId, "capture");

                if (paymentCompletion == null)
                {
                    return new ObjectResult(null);
                }
            }

            var paymentCompletionDto = _mapper.Map<PaymentCompletionDto>(paymentCompletion);
            return new ObjectResult(paymentCompletionDto);
        }

        public async Task TryBroadcastActiveTransactions()
        {
            var count = await _paymentCompletionRepository.CountActive();
            await _counterPushUpdateService.TryPushActiveTransactionsCounter(count);
        }
    }
}
