using System;
using NxB.Dto.AutomationApi;
using NxB.BookingApi.Models;
using Microsoft.Extensions.DependencyInjection;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Model;
using NxB.Clients.Interfaces;
using NxB.Dto.OrderingApi;
using NxB.Settings.Shared.Infrastructure;
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

        public async Task EventLogged(AutomationEventLogItemDto automationEventLog)
        {
            //if (throw new NotImplementedException();
            //    )
        }

        public async Task GateInEventLogged(AutomationEventLogItemDto automationEventLog)
        {
            if (automationEventLog.SubOrderId != null && automationEventLog.Start?.Date == DateTime.Now.Date)
            {
                var tenantId = automationEventLog.TenantId;
                var automationSettingsDto = SettingsRepository.GetAutomationSettings(tenantId);
                if (automationSettingsDto == null || automationSettingsDto.Gates.Count == 0 || !automationSettingsDto.IsSetArrivedOnGateInEnabled) return;

                await AllocationStateClient.AuthorizeClient(tenantId);

                var state = await AllocationStateClient.FindSingleOrDefault(automationEventLog.SubOrderId.Value);
                if (state != null && state.ArrivalStatus == ArrivalStatus.NotArrived || state.ArrivalStatus == ArrivalStatus.DelayedArrival)
                {
                    await AllocationStateClient.AuthorizeClient(tenantId);
                    await AllocationStateClient.AddArrivalState(new AddAllocationStateDto
                    {
                        SubOrderId = automationEventLog.SubOrderId.Value,
                        Status = AllocationStatus.Arrived,
                        Text = $"(BOM) Automatisk sat til ankommet"
                    });

                    await GroupedBroadcasterClient.TryShowToast(new ToastDto
                    {
                        DurationSeconds = automationSettingsDto.Gates[0].NotifyOnErrorSeconds,
                        Text = $"{(automationEventLog.CustomerName ?? "")}, booking {automationEventLog.FriendlyOrderId.Value.DefaultIdPadding()}, er markeret som ankommet",
                        Style = "success"
                    }, tenantId);

                }
            }
        }

        public async Task GateOutEventLogged(AutomationEventLogItemDto automationEventLog) { }
    }
}