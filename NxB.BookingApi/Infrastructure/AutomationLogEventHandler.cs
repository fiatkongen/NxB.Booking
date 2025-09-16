using System;
using NxB.Dto.AutomationApi;
using NxB.MemCacheActor.Interfaces;
using NxB.BookingApi.Models;
using Microsoft.Extensions.DependencyInjection;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Model;
using NxB.Dto.Clients;
using NxB.Dto.OrderingApi;
using NxB.Settings.Shared.Infrastructure;
using ServiceStack.AsyncEx;
using NxB.Dto.Shared;
using Munk.Utils.Object;

namespace NxB.BookingApi.Infrastructure
{
    public class AutomationLogEventHandler : IAutomationLogEvents
    {
        private readonly IServiceProvider _serviceProvider;
        private IGroupedBroadcasterClient GroupedBroadcasterClient => _serviceProvider.GetService<IGroupedBroadcasterClient>();
        private IOrderRepository OrderRepository => _serviceProvider.GetService<IOrderRepository>();
        private IAllocationStateClient AllocationStateClient => _serviceProvider.GetService<IAllocationStateClient>();
        private ISettingsRepository SettingsRepository => _serviceProvider.GetService<ISettingsRepository>();

        public AutomationLogEventHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void EventLogged(AutomationEventLogItemDto automationEventLog)
        {
            //if (throw new NotImplementedException();
            //    )
        }

        public void GateInEventLogged(AutomationEventLogItemDto automationEventLog)
        {
            if (automationEventLog.SubOrderId != null && automationEventLog.Start?.Date == DateTime.Now.Date)
            {
                var tenantId = automationEventLog.TenantId;
                var automationSettingsDto = SettingsRepository.GetAutomationSettings(tenantId);
                if (automationSettingsDto == null || automationSettingsDto.Gates.Count == 0 || !automationSettingsDto.IsSetArrivedOnGateInEnabled) return;

                AllocationStateClient.AuthorizeClient(tenantId).WaitAndUnwrapException();
                
                var state = AllocationStateClient.FindSingleOrDefault(automationEventLog.SubOrderId.Value).WaitAndUnwrapException();
                if (state != null && state.ArrivalStatus == ArrivalStatus.NotArrived || state.ArrivalStatus == ArrivalStatus.DelayedArrival)
                {
                    AllocationStateClient.AuthorizeClient(tenantId).WaitAndUnwrapException();
                    AllocationStateClient.AddArrivalState(new AddAllocationStateDto
                    {
                        SubOrderId = automationEventLog.SubOrderId.Value,
                        Status = AllocationStatus.Arrived,
                        Text = $"(BOM) Automatisk sat til ankommet"
                    });

                    GroupedBroadcasterClient.TryShowToast(new ToastDto
                    {
                        DurationSeconds = automationSettingsDto.Gates[0].NotifyOnErrorSeconds,
                        Text = $"{(automationEventLog.CustomerName ?? "")}, booking {automationEventLog.FriendlyOrderId.Value.DefaultIdPadding()}, er markeret som ankommet",
                        Style = "success"
                    }, tenantId).WaitAndUnwrapException();

                }
            }
        }

        public void GateOutEventLogged(AutomationEventLogItemDto automationEventLog) { }
    }
}