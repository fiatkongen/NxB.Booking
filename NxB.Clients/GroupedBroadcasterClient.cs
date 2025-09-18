using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using NxB.Domain.Common.Constants;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.AutomationApi;
using NxB.Clients.Interfaces;
using NxB.Dto.DocumentApi;
using NxB.Dto.JobApi;
using NxB.Dto.Shared;
using ServiceStack;

namespace NxB.Clients
{
    public class GroupedBroadcasterClient : NxBAdministratorClientWithTenantUrlLookup, IGroupedBroadcasterClient
    {
        private readonly TelemetryClient _telemetryClient;

        public GroupedBroadcasterClient(TelemetryClient telemetryClient, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _telemetryClient = telemetryClient;
        }

        public async Task UpdateCounter(string nameCounter, int valueCounter, Guid? tenantId = null)
        {
            var url = AppendTenantId($"/NxB.Services.App/NxB.SignalrApi/groupedbroadcaster/updatecounter?nameCounter={nameCounter}&valueCounter={valueCounter}", tenantId);
            _telemetryClient.TrackTrace("GroupedBroadcasterClient.UpdateCounter: url=" + url);
            await this.PostAsync(url, null);
        }

        public async Task TryUpdateCounter(string nameCounter, int valueCounter, Guid? tenantId = null)
        {
            try
            {
                await UpdateCounter(nameCounter, valueCounter, tenantId);
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackException(exception);
            }
        }

        public async Task TryUpdateBatchTotals(Guid? tenantId = null)
        {
            try
            {
                var url = AppendTenantId($"/NxB.Services.App/NxB.SignalrApi/groupedbroadcaster/batchtotals", tenantId);
                await this.PutAsync(url, null);
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackException(exception);
            }

        }

        public async Task TryUpdateMessageDeliveryStatus(Guid messageId, DeliveryStatus deliveryStatus, DeliveryStatus smsDeliveryStatus, Guid? tenantId = null)
        {
            try
            {
                var url = AppendTenantId($"/NxB.Services.App/NxB.SignalrApi/groupedbroadcaster/message/deliverystatus?messageId={messageId}&deliveryStatus={deliveryStatus}&smsDeliveryStatus={smsDeliveryStatus}", tenantId);
                await this.PutAsync(url, null);
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackException(exception);
            }
        }

        public async Task TryEmailReceived(Guid? tenantId = null)
        {
            try
            {
                var url = AppendTenantId($"/NxB.Services.App/NxB.SignalrApi/groupedbroadcaster/emailreceived", tenantId);
                await this.PostAsync(url, null);
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackException(exception);
            }
        }

        public async Task TryJobTaskModified(JobTaskDto jobTaskDto, Guid? tenantId = null)
        {
            try
            {
                var url = AppendTenantId($"/NxB.Services.App/NxB.SignalrApi/groupedbroadcaster/jobtaskmodified", tenantId);
                await this.PostAsync(url, jobTaskDto);

                //broadcast to demonstration
                if (tenantId.HasValue && tenantId.Value != AppConstants.DEMONSTRATION_TENANT_ID)
                {
                    url = AppendTenantId($"/NxB.Services.App/NxB.SignalrApi/groupedbroadcaster/jobtaskmodified", AppConstants.DEMONSTRATION_TENANT_ID);
                    await this.PostAsync(url, jobTaskDto);
                }
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackException(exception);
            }
        }

        public async Task TryBatchItemModified(Guid? batchItemId, Guid? messageId, Guid? tenantId = null)
        {
            try
            {
                if (batchItemId == null && messageId == null) return;
                var url = AppendTenantId($"/NxB.Services.App/NxB.SignalrApi/groupedbroadcaster/batchitemmodified?{ (batchItemId != null ? $"batchItemId={batchItemId.Value}" : $"messageId={messageId.Value}")}", tenantId);
                await this.PostAsync(url, null);

                //broadcast to demonstration
                if (tenantId.HasValue && tenantId.Value != AppConstants.DEMONSTRATION_TENANT_ID)
                {
                    url = AppendTenantId($"/NxB.Services.App/NxB.SignalrApi/groupedbroadcaster/batchitemmodified?{ (batchItemId != null ? $"batchItemId={batchItemId.Value}" : $"messageId={messageId.Value}")}", AppConstants.DEMONSTRATION_TENANT_ID);
                    await this.PostAsync(url, null);
                }
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackException(exception);
            }
        }

        public async Task TryTriggerRefreshCounter(string nameCounter)
        {
            try
            {
                var url = $"/NxB.Services.App/NxB.SignalrApi/groupedbroadcaster/triggerrefreshcounter?nameCounter={nameCounter}";
                await this.PostAsync(url, null);
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackException(exception);
            }
        }

        public async Task TryOrderModified(Guid orderId)
        {
            try
            {
                var url = $"/NxB.Services.App/NxB.SignalrApi/groupedbroadcaster/ordermodified?orderId={orderId}";
                await this.PostAsync(url, null);
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackException(exception);
            }
        }

        public async Task TryAutomationEventTriggered(AutomationEventLogItemDto itemDto, Guid tenantId)
        {
            try
            {
                var url = $"/NxB.Services.App/NxB.SignalrApi/groupedbroadcaster/automationeventtriggered?tenantId={tenantId}";
                await this.PostAsync(url, itemDto);
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackException(exception);
            }
        }

        public async Task TryShowToast(ToastDto toastDto, Guid tenantId)
        {
            try
            {
                var url = $"/NxB.Services.App/NxB.SignalrApi/groupedbroadcaster/showtoast?tenantId={tenantId}";
                await this.PostAsync(url, toastDto);
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackException(exception);
            }
        }

        private string AppendTenantId(string url, Guid? tenantId)
        {
            return url + (tenantId != null ? (url.Contains('?') ? "&" : "?") + "tenantId=" + tenantId.Value : "");
        }
    }
}
