using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NxB.Dto.OrderingApi;
// TODO: Replace Service Fabric MemCacheActor functionality
// using NxB.MemCacheActor.Interfaces;
// TODO: Remove old namespace references
// using NxB.PricingApi.Infrastructure;
using NxB.BookingApi.Infrastructure;

namespace NxB.BookingApi.Models
{
    // TODO: Replace Service Fabric MemCacheActor functionality
    // public class CacheUpdatedHandler : IMemCacheActorEvents
    public class CacheUpdatedHandler
    {
        private readonly IServiceProvider _serviceProvider;

        public CacheUpdatedHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // TODO: Replace Service Fabric MemCacheActor functionality
        // public void CacheUpdated(Guid tenantId, string cacheName)
        // {
        //     IPriceCalculator priceCalculator = (IPriceCalculator) _serviceProvider.GetService(typeof(IPriceCalculator));
        //     priceCalculator?.ClearCaches(tenantId);
        // }
    }
}