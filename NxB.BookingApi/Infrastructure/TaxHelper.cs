using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using Munk.Utils.Object;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.Clients;
using NxB.Settings.Shared.Infrastructure;

namespace NxB.BookingApi.Infrastructure
{
    public class TaxHelper : ITaxHelper
    {
        private readonly ITaxClient _taxClient;
        private readonly ISettingsRepository _settingsRepository;
        private readonly TelemetryClient _telemetryClient;

        public TaxHelper(ITaxClient taxClient, ISettingsRepository settingsRepository, TelemetryClient telemetryClient)
        {
            _taxClient = taxClient;
            _settingsRepository = settingsRepository;
            _telemetryClient = telemetryClient;
        }

        public async Task AddTaxToOrder(List<ITaxableItem> taxableItems, DbContext dbContext)
        {
            if (_settingsRepository.IsIndividualTaxEnabled())
            {
                try
                {
                    var resourceIds = taxableItems.Select(x => x.ResourceId).Distinct().ToList();

                    var mappedTaxes = await _taxClient.MapTaxesFromResources(resourceIds);
                    var generalTax = _settingsRepository.TaxRate();

                    taxableItems.ForEach(x =>
                    {
                        if (mappedTaxes.ContainsKey(x.ResourceId))
                        {
                            x.TaxPercent = mappedTaxes[x.ResourceId];
                        }
                        else if (generalTax != null)
                        {
                            x.TaxPercent = generalTax.Value;
                        }

                        x.Tax = x.Total.ReverseTaxAmount(x.TaxPercent);
                    });
                }
                catch (Exception exception)
                {
                    _telemetryClient.TrackException(exception);
                }
            }
        }

        public async Task UpdateTaxForOrder(List<ITaxableItem> taxableItems, DbContext dbContext)
        {
            if (_settingsRepository.IsIndividualTaxEnabled())
            {
                try
                {
                    if (taxableItems.Count == 0) return;

                    var resourceIds = taxableItems.Select(x => x.ResourceId).Distinct().ToList();
                    var mappedTaxes = await _taxClient.MapTaxesFromResources(resourceIds);
                    var generalTax = _settingsRepository.TaxRate();

                    taxableItems.ForEach(x =>
                    {
                        if (mappedTaxes.ContainsKey(x.ResourceId))
                        {
                            x.TaxPercent = mappedTaxes[x.ResourceId];
                        }
                        else if (generalTax != null)
                        {
                            x.TaxPercent = generalTax.Value;
                        }

                        x.Tax = x.Total.ReverseTaxAmount(x.TaxPercent);
                    });
                }
                catch (Exception exception)
                {
                    _telemetryClient.TrackException(exception);
                }
            }
        }
    }
}
