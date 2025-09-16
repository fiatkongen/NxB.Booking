using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickPay.SDK.Models.Payments;
using Polly;
using QuickPay.SDK;
using NxB.BookingApi.Exceptions;

namespace NxB.BookingApi.Infrastructure
{
    public interface IPaymentLinkHelper
    {
        void LogPaymentLinkCreated();
        void LogPaymentLinkCreationError();
        void LogPaymentLinkRetry();
        Task<PaymentLinkUrl> CreatePaymentLink(Func<Task<PaymentLinkUrl>> createPaymentLinkFunction);
    }

    public class PaymentLinkHelper : IPaymentLinkHelper
    {
        private readonly TelemetryClient _telemetryClient;

        public PaymentLinkHelper(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        public void LogPaymentLinkCreated()
        {
            var metrics = new Dictionary<string, double>
            {
                { "PaymentLinkCreated", 1 },
            };

            _telemetryClient.TrackEvent("PaymentLinkMetrics", metrics: metrics);
        }

        public void LogPaymentLinkCreationError()
        {
            var metrics = new Dictionary<string, double>
            {
                { "PaymentLinkCreationError", 1 },
            };

            _telemetryClient.TrackEvent("PaymentLinkMetrics", metrics: metrics);
        }

        public void LogPaymentLinkRetry()
        {
            var metrics = new Dictionary<string, double>
            {
                { "PaymentLinkRetry", 1 },
            };

            _telemetryClient.TrackEvent("PaymentLinkMetrics", metrics: metrics);
        }

        public async Task<PaymentLinkUrl> CreatePaymentLink(Func<Task<PaymentLinkUrl>> createPaymentLinkFunction)
        {
            PaymentLinkUrl link = null;

            var retryPolicy = Policy.Handle<Exception>().OrResult<PaymentLinkUrl>(l => l.Url == null)
                .WaitAndRetryAsync(retryCount: 5, sleepDurationProvider: _ => TimeSpan.FromSeconds(2),
                    onRetry: (exception, sleepDuration, attemptNumber, context) =>
                    {
                        _telemetryClient.TrackTrace($"PaymentLinkService.QuickPay (retry {attemptNumber}) error {exception}");
                        this.LogPaymentLinkRetry();
                    });
            await retryPolicy.ExecuteAsync(async () =>
            {
                link = await createPaymentLinkFunction().ConfigureAwait(false);
                return link;
            });

            if (link.Url == null)
            {
                LogPaymentLinkCreationError();
                throw new PaymentLinkException("PaymentLinkService.CreatePaymentLink. Could not create Link");
            }

            return link;
        }
    }
}
