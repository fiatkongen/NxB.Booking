using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NxB.Dto.Clients;
using NxB.MemCacheActor.Interfaces;
using NxB.Settings.Shared.Infrastructure;
using ServiceStack.AsyncEx;

namespace NxB.BookingApi.Infrastructure
{
    public class PriceProfileModifiedHandler : IPriceProfileActorEvents
    {
        private readonly IServiceProvider _serviceProvider;

        public PriceProfileModifiedHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void PriceProfilesUpdated(Guid tenantId, List<Guid> ids)
        {
            var settingsRepository = _serviceProvider.GetService<ISettingsRepository>();

            if (settingsRepository.IsCtoutvertActivated(tenantId))
            {
                var priceProfileClient = _serviceProvider.GetService<IPriceProfileClient>();
                priceProfileClient.AuthorizeClient(tenantId).WaitAndUnwrapException();

                var rentalCategoryClient = _serviceProvider.GetService<IRentalCategoryClient>();
                rentalCategoryClient.AuthorizeClient(tenantId).WaitAndUnwrapException();

                ids = ids.Distinct().ToList();
                var rentalCategoryIds = new List<Guid>();

                foreach (var priceprofileId in ids)
                {
                    var priceProfile = priceProfileClient.FindSingle(priceprofileId).WaitAndUnwrapException();
                    var rentalCategoryDto = rentalCategoryClient.FindSingleOrDefault(priceProfile.ResourceId).WaitAndUnwrapException();
                    if (rentalCategoryDto != null)
                    {
                        rentalCategoryIds.Add(rentalCategoryDto.Id);
                    }
                }

                if (rentalCategoryIds.Count > 0)
                {
                    var ctoutvertPriceAvailabilityClient = _serviceProvider.GetService<ICtoutvertClient>();
                    ctoutvertPriceAvailabilityClient.AuthorizeClient(tenantId).WaitAndUnwrapException();
                    ctoutvertPriceAvailabilityClient.PushPriceAvailability(tenantId, rentalCategoryIds.Distinct().ToList(), true).WaitAndUnwrapException();
                }
            }
        }
    }
}
