using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Munk.AspNetCore;
using Munk.Utils.Object;
using Newtonsoft.Json;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Model;
using NxB.Dto.AccountingApi;
using NxB.Clients.Interfaces;
using NxB.Settings.Shared.Infrastructure;
using Polly;
using QuickPay.SDK.Models.Payments;
using Payment = QuickPay.SDK.Models.Payments.Payment;

namespace NxB.BookingApi.Controllers.Accounting
{
    [Produces("application/json")]
    [Route("paymentlinkonline")]
    [ApiValidationFilter]
    public class PaymentLinkOnlineController : BaseController
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ITenantClient _tenantClient;
        private readonly AppDbContext _appDbContext;
        private readonly IPaymentLinkHelper _paymentLinkHelper;
        private readonly IPaymentLinkService _paymentLinkService;
        private readonly IOrderRepository _orderRepository;

        public PaymentLinkOnlineController(
            TelemetryClient telemetryClient,
            ITenantClient tenantClient, AppDbContext appDbContext, IPaymentLinkHelper paymentLinkHelper, IPaymentLinkService paymentLinkService, IOrderRepository orderRepository)
        {
            _telemetryClient = telemetryClient;
            _tenantClient = tenantClient;
            _appDbContext = appDbContext;
            _paymentLinkHelper = paymentLinkHelper;
            _paymentLinkService = paymentLinkService;
            _orderRepository = orderRepository;
        }


        [HttpPost]
        [Route("")]
        [AllowAnonymous]
        public async Task<ObjectResult> CreateOnlinePaymentLink([FromBody] CreateOnlinePaymentLinkDto dto)
        {
            _telemetryClient.TrackTrace(
                $"QuickPay.CreateOnlinePaymentLink {JsonConvert.SerializeObject(dto)}");

            var tenantId = dto.TenantId;
            var tenantDto = await _tenantClient.FindTenantFromId(tenantId);
            var campname = tenantDto.CompanyName;
            string orderId;

            if (long.TryParse(dto.OrderId, out var friendlyOrderId))
            {
                var orderRepository = _orderRepository.CloneWithCustomClaimsProvider(TemporaryClaimsProvider.CreateAdministrator(tenantId));
                var order = await orderRepository.FindSingleFromFriendlyId(friendlyOrderId, false);
                orderId = order.Id.ToString();
            }
            else
            {
                orderId = dto.OrderId;
            }
            var amount = dto.Amount;
            var existingPaymentId = dto.ExistingPaymentId;
            var isLocalhost = dto.HostName.LastIndexOf("localhost", StringComparison.Ordinal) > -1;
            var continueUrl = $"{CreateBaseUrl(dto, isLocalhost)}/payment-completed/{tenantId}?orderId={orderId}&friendlyOrderId={friendlyOrderId}{dto.ContinueUrlParameters ?? ""}";
            var cancelUrl = CreateBaseUrl(dto, isLocalhost) + "/search/" + tenantId + (dto.CancelUrlParameters ?? "");

            if (dto.OverrideContinueUrl != null)
            {
                continueUrl = dto.OverrideContinueUrl == "" ? null : CreateBaseUrl(dto, isLocalhost) + dto.OverrideContinueUrl + "?orderId=" + orderId + (dto.ContinueUrlParameters ?? "");
            }

            if (dto.OverrideCancelUrl != null)
            {
                cancelUrl = dto.OverrideCancelUrl == "" ? null : CreateBaseUrl(dto, isLocalhost) + dto.OverrideCancelUrl + "?orderId=" + orderId + (dto.CancelUrlParameters ?? "");
            }

            var preferredLanguageId = dto.Language;
            var amountInt = decimal.ToInt32(amount * 100);

            var temporaryClaimsProvider = TemporaryClaimsProvider.CreateOnline(tenantId);
            var settingsRepository = new SettingsRepository<AppDbContext>(temporaryClaimsProvider, _appDbContext);

            var quickPayClient = PaymentLinkService.CreateQuickPayClient(tenantId, settingsRepository);
            var quickPayOrderId = dto.QuickPayOrderId;
            PaymentLinkUrl paymentLink = null;

            if (existingPaymentId == null)
            {
                var applicationCurrency = settingsRepository.GetApplicationCurrency();
                var applicationLanguage = preferredLanguageId ?? settingsRepository.GetApplicationLanguage();
                string paymentMethods = settingsRepository.GetOnlineBookingPaymentProviders(tenantId).BuildQuickPayPaymentMethodsString();
                var quickPaySettings = settingsRepository.GetQuickPaySettings(tenantId);
                var paymentLinkId = Guid.NewGuid();

                var isFeeAdded = quickPaySettings.IsQuickPayOnlineLinkAutoFee;
                var isAutoCaptured = quickPaySettings.IsQuickPayOnlineLinkAutoCaptured;

                Payment payment = null;

                var retryPolicy = Policy.Handle<Exception>().OrResult<Payment>(x => x.Id == 0)
                    .WaitAndRetryAsync(retryCount: 5, sleepDurationProvider: _ => TimeSpan.FromSeconds(5),
                        onRetry: (exception, sleepDuration, attemptNumber, context) =>
                        {
                            _telemetryClient.TrackTrace($"PaymentLinkService.QuickPay (retry {attemptNumber}) error {exception}");
                            _paymentLinkHelper.LogPaymentLinkRetry();
                        });

                await retryPolicy.ExecuteAsync(async () =>
                {
                    payment = await quickPayClient.Payments.Create(applicationCurrency, quickPayOrderId,
                    new Dictionary<string, string>
                    {
                        {"tenantId", tenantId.ToString()},
                        {"campname", campname},
                        {"linkSource", "onlinebooking"},
                        {"paymentLinkId", paymentLinkId.ToString()},
                        {"isFeeAdded", isFeeAdded.ToString()},
                        {"isAutoCaptured", isAutoCaptured.ToString()},
                        {"preferredLanguageId", preferredLanguageId},
                        {"orderId", orderId},
                        {"testMode", dto.TestMode.ToString()},
                        {"cartJson", dto.Cart != null ? JsonConvert.SerializeObject(dto.Cart) : null},
                        {"amount", amount.ToString(CultureInfo.InvariantCulture)},
                    }).ConfigureAwait(false);
                    return payment;
                });

                if (payment.Id == 0)
                    throw new PaymentLinkException(
                        "PaymentLinkOnlineController.CreateOnlinePaymentLink. Could not create Payment. Return JSON: " + JsonConvert.SerializeObject(payment));
                existingPaymentId = payment.Id;

                //remember callback url is also used for authorize and capture
                //var callbackUrl = "https://api.next-stay-booking.dk/accountingapi/paymentlink/callback";
                var callbackUrl = "";

                paymentLink = await _paymentLinkHelper.CreatePaymentLink(async () => await quickPayClient.Payments.CreateOrUpdatePaymentLink(payment.Id, amountInt, isAutoCaptured,
                    isFeeAdded, applicationLanguage, paymentMethods, continueUrl + "&paymentId=" + payment.Id, cancelUrl,
                    callbackUrl, false));
            }
            else
            {
                paymentLink = await _paymentLinkHelper.CreatePaymentLink(async () => await quickPayClient.Payments.CreateOrUpdatePaymentLink(existingPaymentId.Value, amountInt));
            }

            _telemetryClient.TrackTrace("PaymentLinkOnlineController Created paymentlink with QuickPayPaymentId: " + existingPaymentId);

            return new ObjectResult(new PaymentLinkOnlineDto
            { PaymentId = existingPaymentId.Value, Url = paymentLink.Url });
        }

        private static string CreateBaseUrl(CreateOnlinePaymentLinkDto dto, bool isLocalhost)
        {
            return (isLocalhost ? "http" : "https") + "://" + dto.HostName;
        }

        [HttpDelete]
        [Route("")]
        [AllowAnonymous]
        public async Task<IActionResult> DeletePaymentLink(int quickPayPaymentId, Guid tenantId)
        {
            await _paymentLinkService.DeletePaymentLink(quickPayPaymentId, tenantId);
            return Ok();
        }
    }

}
