using Microsoft.ApplicationInsights;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Constants;
using NxB.Dto.AllocationApi;
using NxB.Dto.OrderingApi;

namespace NxB.BookingApi.Infrastructure
{
    public static class EventPublisherExtensions
    {
        public static async Task TryPublishOrderModified(this IMemCacheActor memCacheActor, Guid tenantId, OrderDto originalOrderDto, List<SubOrderDto> modifiedSubOrderDto,
            List<SubOrderDto> createdSubOrderDtos, TelemetryClient telemetryClient)
        {
            try
            {
                await memCacheActor.PublishOrderModified(tenantId, originalOrderDto, modifiedSubOrderDto, createdSubOrderDtos);
            }
            catch (Exception exception)
            {
                telemetryClient.TrackTrace("TryPublishOrderModified. error");
                telemetryClient.TrackException(exception);
            }
        }

        public static async Task TryPublishPriceProfilesUpdated(this IMemCacheActor memCacheActor, Guid tenantId, List<Guid> ids, TelemetryClient telemetryClient)
        {
            try
            {
                await memCacheActor.PublishPriceProfilesUpdated(tenantId, ids);
            }
            catch (Exception exception)
            {
                telemetryClient.TrackTrace("TryPublishPriceProfilesUpdated. error");
                telemetryClient.TrackException(exception);
            }
        }

        public static async Task TryPublishRentalCategoryModified(this IMemCacheActor memCacheActor, Guid tenantId, RentalCategoryDto originalRentalCategoryDto,
            RentalCategoryDto modifiedRentalCategoryDto, TelemetryClient telemetryClient)
        {
            try
            {
                await memCacheActor.PublishRentalCategoryModified(tenantId, originalRentalCategoryDto, modifiedRentalCategoryDto);
            }
            catch (Exception exception)
            {
                telemetryClient.TrackTrace("TryPublishRentalCategoryModified. error");
                telemetryClient.TrackException(exception);
            }
        }

        public static async Task TryPublishRentalCategoryCreated(this IMemCacheActor memCacheActor, Guid tenantId, RentalCategoryDto rentalCategoryDto, TelemetryClient telemetryClient)
        {
            try
            {
                await memCacheActor.PublishRentalCategoryCreated(tenantId, rentalCategoryDto);
            }
            catch (Exception exception)
            {
                telemetryClient.TrackTrace("TryPublishRentalCategoryCreated. error");
                telemetryClient.TrackException(exception);
            }
        }

        public static async Task TryPublishRentalCategoryDeleted(this IMemCacheActor memCacheActor, Guid tenantId, Guid rentalCategoryId, TelemetryClient telemetryClient)
        {
            try
            {
                await memCacheActor.PublishRentalCategoryDeleted(tenantId, rentalCategoryId);
            }
            catch (Exception exception)
            {
                telemetryClient.TrackTrace("TryPublishRentalCategoryDeleted. error");
                telemetryClient.TrackException(exception);
            }
        }

        public static async Task TryPublishRentalCategoryUnDeleted(this IMemCacheActor memCacheActor, Guid tenantId, Guid rentalCategoryId, TelemetryClient telemetryClient)
        {
            try
            {
                await memCacheActor.PublishRentalCategoryUnDeleted(tenantId, rentalCategoryId);
            }
            catch (Exception exception)
            {
                telemetryClient.TrackTrace("TryPublishRentalCategoryUnDeleted. error");
                telemetryClient.TrackException(exception);
            }
        }

        public static async Task TryPublishTimeSpansUpdated(this IMemCacheActor memCacheActor, Guid tenantId, List<TimeSpanDto> timeSpansCreated, List<TimeSpanDto> timeSpansModified, List<TimeSpanDto> timeSpansDeleted, TelemetryClient telemetryClient)
        {
            try
            {
                await memCacheActor.PublishTimeSpansUpdated(tenantId, timeSpansCreated, timeSpansModified, timeSpansDeleted);
            }
            catch (Exception exception)
            {
                telemetryClient.TrackTrace("TryPublishRentalCategoryUnDeleted. error");
                telemetryClient.TrackException(exception);
            }
        }

        public static async Task TryPublishCacheUpdated(this IMemCacheActor memCacheActor, Guid tenantId, TelemetryClient telemetryClient)
        {
            try
            {
                await memCacheActor.PublishCacheUpdated(tenantId, AppConstants.CACHE_COST_UPDATED);
            }
            catch (Exception exception)
            {
                telemetryClient.TrackTrace("TryPublishCacheUpdated. error");
                telemetryClient.TrackException(exception);
            }
        }
    }
}