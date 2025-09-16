using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Munk.Utils.Object;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;
using NxB.Dto.AccountingApi;
using NxB.Settings.Shared.Infrastructure;
using Polly;
using QuickPay.SDK;
using QuickPay.SDK.Clients;
using QuickPay.SDK.Models.Callbacks;
using QuickPay.SDK.Models.Payments;
using Payment = QuickPay.SDK.Models.Payments.Payment;
using PaymentLink = NxB.AccountingApi.Model.PaymentLink;

namespace NxB.BookingApi.Infrastructure
{
    public class PaymentLinkService : IPaymentLinkService
    {
        private readonly PaymentLinkFactory _paymentLinkFactory;
        private readonly ISettingsRepository _settingsRepository;
        private readonly TelemetryClient _telemetryClient;
        private readonly IPaymentLinkRepository _paymentLinkRepository;
        private readonly IClaimsProvider _claimsProvider;
        private readonly IPaymentLinkHelper _paymentLinkHelper;

        public PaymentLinkService(PaymentLinkFactory paymentLinkFactory, ISettingsRepository settingsRepository, TelemetryClient telemetryClient, IPaymentLinkRepository paymentLinkRepository, IClaimsProvider claimsProvider, IPaymentLinkHelper paymentLinkHelper)
        {
            _paymentLinkFactory = paymentLinkFactory;
            _settingsRepository = settingsRepository;
            _telemetryClient = telemetryClient;
            _paymentLinkRepository = paymentLinkRepository;
            _claimsProvider = claimsProvider;
            _paymentLinkHelper = paymentLinkHelper;
        }

        public async Task<PaymentLink> CreatePaymentLink(long friendlyVoucherId, VoucherType voucherType, long friendlyOrderId, decimal amount, PaymentLinkTestMode testMode)
        {
            try
            {
                _telemetryClient.TrackTrace($"QuickPay.CreatePaymentLink friendlyVoucherId={friendlyVoucherId}, amount={amount}, testMode={testMode}");
                var applicationCurrency = _settingsRepository.GetApplicationCurrency();
                var applicationLanguage = _settingsRepository.GetApplicationLanguage();

                QuickPayClient quickPayClient = CreateQuickPayClient();
                var paymentLinkId = Guid.NewGuid();

                var quickPaySettings = _settingsRepository.GetQuickPaySettings(_claimsProvider.GetTenantId());
                string paymentMethods = _settingsRepository.GetOnlineBookingPaymentProviders(_claimsProvider.GetTenantId()).BuildQuickPayPaymentMethodsString();
                var quickPayOrderId = friendlyOrderId.DefaultIdPadding() + "_" + ((int)voucherType) + "_" + friendlyVoucherId.DefaultIdPadding();

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
                            {"tenantId", _claimsProvider.GetTenantId().ToString()},
                            {"paymentLinkId", paymentLinkId.ToString()},
                            {"linkSource", "paymentlink"},
                            {"linkType", LinkType.Voucher.ToString()},
                            {"isFeeAdded", quickPaySettings.IsQuickPayPaymentLinkAutoFee.ToString()},
                            {"isAutoCaptured", quickPaySettings.IsQuickPayPaymentLinkAutoCaptured.ToString()},
                            {"friendlyOrderId", friendlyOrderId.ToString()},
                            {"friendlyVoucherId", friendlyVoucherId.ToString()},
                            {"testMode", testMode.ToString()},
                            {"amount", amount.ToString(CultureInfo.InvariantCulture)},
                            {"voucherType", ((int)voucherType).ToString(CultureInfo.InvariantCulture)},
                        }).ConfigureAwait(false);
                    return payment;
                });
                

                if (payment.Id == 0)
                    throw new PaymentLinkException("PaymentLinkService.CreatePaymentLink. Could not create Payment");

                var callbackUrl = "https://api.next-stay-booking.dk/accountingapi/paymentlink/callback";

                PaymentLinkUrl link = await _paymentLinkHelper.CreatePaymentLink(async () => await quickPayClient.Payments.CreateOrUpdatePaymentLink(payment.Id, Convert.ToInt32(amount * 100), quickPaySettings.IsQuickPayPaymentLinkAutoCaptured, quickPaySettings.IsQuickPayPaymentLinkAutoFee, applicationLanguage, paymentMethods, null, null, callbackUrl, false));
                
                var paymentLink = _paymentLinkFactory.Create(paymentLinkId, friendlyVoucherId, voucherType, friendlyOrderId, payment.Id, amount, link.Url);

                _telemetryClient.TrackTrace($"QuickPay.CreatePaymentLink created url={link.Url}");

                await _paymentLinkRepository.Add(paymentLink);

                _paymentLinkHelper.LogPaymentLinkCreated();

                return paymentLink;
            }
            catch (Exception exception)
            {
                this._telemetryClient.TrackException(exception);
                _paymentLinkHelper.LogPaymentLinkCreationError();
                throw new PaymentLinkException("QuickPay.CreatePaymentLink error: " + exception.Message);
            }
        }

        public QuickPayClient CreateQuickPayClient(Guid? tenantId = null)
        {
            var quickPaySettings = _settingsRepository.GetQuickPaySettings(tenantId);
            return new QuickPayClient(quickPaySettings.QuickPayApiUser, quickPaySettings.QuickPayPrivateKey, quickPaySettings.QuickPayUserKey);
        }

        public static QuickPayClient CreateQuickPayClient(Guid tenantId, ISettingsRepository settingsRepository)
        {
            var quickPaySettings = settingsRepository.GetQuickPaySettings(tenantId);
            return new QuickPayClient(quickPaySettings.QuickPayApiUser, quickPaySettings.QuickPayPrivateKey, quickPaySettings.QuickPayUserKey);
        }

        public bool ValidateRequest(string requestBody, string checkSum, Guid tenantId)
        {
            var quickPaySettings = _settingsRepository.GetQuickPaySettings(tenantId);
            string compute = Sign(requestBody, quickPaySettings.QuickPayPrivateKey);
            return checkSum != null && checkSum.Equals(compute);
        }

        public async Task<bool> DeletePaymentLink(int quickPayPaymentId, Guid tenantId)
        {
            QuickPayClient quickPayClient = CreateQuickPayClient(tenantId);
            var result = await quickPayClient.Payments.DeletePaymentLink(quickPayPaymentId);
            return result;
        }

        private string Sign(string baseString, string api_key)
        {
            var e = Encoding.UTF8;

            var hmac = new HMACSHA256(e.GetBytes(api_key));
            byte[] b = hmac.ComputeHash(e.GetBytes(baseString));

            var s = new StringBuilder();
            for (int i = 0; i < b.Length; i++)
            {
                s.Append(b[i].ToString("x2"));
            }

            return s.ToString();
        }
    }
}
