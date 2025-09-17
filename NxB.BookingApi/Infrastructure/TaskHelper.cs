using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace NxB.BookingApi.Infrastructure
{
    //https://mikhail.io/2016/01/fire-and-forget-in-service-fabric-actors/
    //https://stackoverflow.com/questions/35136801/configureawait-on-which-thread-is-the-exception-handled
    public static class TaskHelper
    {

        public static async Task CatchExceptionAndLogToTelemetry(this Task task, TelemetryClient telemetryClient)
        {
            try
            {
                await task;
            }
            catch (Exception exception)
            {
                telemetryClient.TrackTrace("CatchExceptionAndLogToTelemetry");
                telemetryClient.TrackException(exception);
            }
        }

        public static void FireAndForget(this Task task)
        {
            Task.Run(async () => await task).ConfigureAwait(false);
        }

        public static void FireAndForgetLogToTelemetry(this Task task, TelemetryClient telemetryClient)
        {
            Task.Run(async () =>
            {
                try
                {
                    await task;
                }
                catch (Exception exception)
                {
                    telemetryClient.TrackTrace("FireAndForgetLogToTelemetry");
                    telemetryClient.TrackException(exception);
                }
            }).ConfigureAwait(false);
        }

        [Obsolete]
        public static void FireAndForgetLogToTelemetry(this Task task, string instrumentationKey)
        {
            var telemetryClient = new TelemetryClient(new TelemetryConfiguration(instrumentationKey));

            Task.Run(async () =>
            {
                try
                {
                    await task;
                }
                catch (Exception exception)
                {
                    telemetryClient.TrackTrace("FireAndForgetLogToTelemetry");
                    telemetryClient.TrackException(exception);
                }
            }).ConfigureAwait(false);
        }
    }
}
