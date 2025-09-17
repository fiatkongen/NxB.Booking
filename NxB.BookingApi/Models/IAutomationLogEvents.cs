using NxB.Dto.AutomationApi;

namespace NxB.BookingApi.Models
{
    public interface IAutomationLogEvents
    {
        Task EventLogged(AutomationEventLogItemDto automationEventLog);
        Task GateInEventLogged(AutomationEventLogItemDto automationEventLog);
        Task GateOutEventLogged(AutomationEventLogItemDto automationEventLog);
    }
}