using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NxB.Clients.Interfaces;
using NxB.Settings.Shared.Infrastructure;

namespace NxB.BookingApi.Infrastructure
{
    public class PriceProfileModifiedHandler
    {
        private readonly IServiceProvider _serviceProvider;

        public PriceProfileModifiedHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task PriceProfilesUpdated(Guid tenantId, List<Guid> ids)
        {
            var settingsRepository = _serviceProvider.GetService<ISettingsRepository>();

            if (settingsRepository.IsCtoutvertActivated(tenantId))
            {
                var priceProfileClient = _serviceProvider.GetService<IPriceProfileClient>();
                await priceProfileClient.AuthorizeClient(tenantId);

                var rentalCategoryClient = _serviceProvider.GetService<IRentalCategoryClient>();
                await rentalCategoryClient.AuthorizeClient(tenantId);

                ids = ids.Distinct().ToList();
                var rentalCategoryIds = new List<Guid>();

                foreach (var priceprofileId in ids)
                {
                    var priceProfile = await priceProfileClient.FindSingle(priceprofileId);
                    var rentalCategoryDto = await rentalCategoryClient.FindSingleOrDefault(priceProfile.ResourceId);
                    if (rentalCategoryDto != null)
                    {
                        rentalCategoryIds.Add(rentalCategoryDto.Id);
                    }
                }

                if (rentalCategoryIds.Count > 0)
                {
                    var ctoutvertPriceAvailabilityClient = _serviceProvider.GetService<ICtoutvertClient>();
                    await ctoutvertPriceAvailabilityClient.AuthorizeClient(tenantId);
                    await ctoutvertPriceAvailabilityClient.PushPriceAvailability(tenantId, rentalCategoryIds.Distinct().ToList(), true);
                }
            }
        }
    }
}
