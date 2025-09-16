using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NxB.Domain.Common.Enums;
using NxB.Dto.AutomationApi;
using NxB.Dto.DocumentApi;
using NxB.Dto.JobApi;
using NxB.Dto.Shared;

namespace NxB.Dto.Clients
{
    public interface IGroupedBroadcasterClient : IAuthorizeClient
    {
        Task UpdateCounter(string nameCounter, int valueCounter, Guid? tenantId = null); 
        Task TryUpdateCounter(string nameCounter, int valueCounter, Guid? tenantId = null);
        Task TryEmailReceived(Guid? tenantId = null);
        Task TryTriggerRefreshCounter(string nameCounter);
        Task TryOrderModified(Guid orderId);
        Task TryJobTaskModified(JobTaskDto jobTaskDto, Guid? tenantId = null);
        Task TryBatchItemModified(Guid? batchItemId, Guid? messageId, Guid? tenantId = null);
        Task TryUpdateBatchTotals(Guid? tenantId = null);
        Task TryAutomationEventTriggered(AutomationEventLogItemDto itemDto, Guid tenantId);
        Task TryUpdateMessageDeliveryStatus(Guid messageId, DeliveryStatus deliveryStatus, DeliveryStatus smsDeliveryStatus, Guid? tenantId = null);
        Task TryShowToast(ToastDto toastDto, Guid tenantId);
    }
}
