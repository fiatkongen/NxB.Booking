using NxB.Domain.Common.Enums;
using NxB.Dto.AllocationApi;
using NxB.Dto.AutomationApi;
using NxB.Dto.DocumentApi;
using NxB.Dto.LogApi;
using NxB.Dto.OrderingApi;
using NxB.Dto.TallyWebIntegrationApi;

namespace NxB.BookingApi.Models
{
    public interface IMemCacheActor
    {
        Task<string> GetCachedString(string name);
        Task SetCachedString(string name, string value);

        Task<DateTime?> GetCachedDateTime(string name, DateTime? defaultValue);
        Task SetCachedDateTime(string name, DateTime? value);

        Task<bool?> GetCachedBool(string name, bool? defaultValue);
        Task SetCachedBool(string name, bool? value);

        Task<int?> GetCachedInt(string name, int? defaultValue);
        Task SetCachedInt(string name, int? value);

        Task<long?> GetCachedLong(string name, int? defaultValue);
        Task SetCachedLong(string name, long? value);

        Task<decimal?> GetCachedDecimal(string name, decimal? defaultValue);
        Task SetCachedDecimal(string name, decimal? value);

        Task<List<TallyGateDto>> GetCachedGates(string name);
        Task SetCachedGates(string name, List<TallyGateDto> value);
        Task ClearCachedGates();

        Task<long> GenerateNextFriendlyDueDepositId(Guid tenantId);

        Task PublishCacheUpdated(Guid tenantId, string cacheName);

        Task PublishOrderModified(Guid tenantId, OrderDto originalOrderDto, List<SubOrderDto> modifiedSubOrderDto, List<SubOrderDto> createdSubOrderDtos);
        Task PublishOrderCreated(Guid tenantId, OrderDto orderDto);
        Task PublishSubOrderArrivalStateChanged(Guid tenantId, Guid subOrderId, ArrivalStatus orgArrivalState, ArrivalStatus newArrivalState);
        Task PublishSubOrderDepartureStateChanged(Guid tenantId, Guid subOrderId, DepartureStatus orgDepartureState, DepartureStatus newDepartureState);
        Task PublishSubOrderCancelled(Guid tenantId, Guid subOrderId);

        Task PublishPriceProfilesUpdated(Guid tenantId, List<Guid> ids);

        Task PublishRentalCategoryModified(Guid tenantId, RentalCategoryDto originalRentalCategoryDto, RentalCategoryDto modifiedRentalCategoryDto);
        Task PublishRentalCategoryCreated(Guid tenantId, RentalCategoryDto rentalCategoryDto);
        Task PublishRentalCategoryDeleted(Guid tenantId, Guid rentalCategoryId);
        Task PublishRentalCategoryUnDeleted(Guid tenantId, Guid rentalCategoryId);

        Task PublishTimeSpansUpdated(Guid tenantId, List<TimeSpanDto> timeSpansCreated,
            List<TimeSpanDto> timeSpansModified, List<TimeSpanDto> timeSpansDeleted);

        Task PublishAllocationLogCreated(Guid tenantId, ApplicationLogDto applicationLogDto);
        Task PublishAllocationLogModified(Guid tenantId, ApplicationLogDto originalApplicationLogDto,
            ApplicationLogDto modifiedApplicationLogDto);
        Task PublishAllocationLogsModified(Guid tenantId);

        Task PublishAutomationEventLogged(AutomationEventLogItemDto dto);
        Task PublishAutomationGateInEventLogged(AutomationEventLogItemDto dto);
        Task PublishAutomationGateOutEventLogged(AutomationEventLogItemDto dto);
        Task<List<OnlineDocumentTemplateDto>> GetCachedOnlineDocumentTemplates(Guid tenantId);
        Task SetCachedOnlineDocumentTemplates(Guid tenantId, List<OnlineDocumentTemplateDto> templates);
        Task ClearCachedOnlineDocumentTemplates(Guid tenantId);
        Task ClearAllCachedOnlineDocumentTemplates();
        Task<List<OnlineDocumentTemplateDto>> GetCachedOnlineDocumentTemplatesLanguage(Guid tenantId, string language);
        Task SetCachedOnlineDocumentTemplatesLanguage(Guid tenantId, string language, List<OnlineDocumentTemplateDto> templates);
    }
}