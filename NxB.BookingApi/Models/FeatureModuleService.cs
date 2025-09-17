using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Munk.AspNetCore.Sql;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.TenantApi;
using NxB.BookingApi.Exceptions;

namespace NxB.BookingApi.Models
{
    public class FeatureModuleService : IFeatureModuleService
    {
        private readonly FeatureModuleFactory _featureModuleFactory;

        public FeatureModuleService(FeatureModuleFactory featureModuleFactory)
        {
            _featureModuleFactory = featureModuleFactory;
        }

        public async Task<FeatureModuleTenantEntry> ActivateModuleForTrial(Guid tenantId, Guid userId, FeatureModule featureModule, List<FeatureModuleTenantEntry> featureModuleTenantEntries)
        {
            var alreadyActivated = featureModuleTenantEntries.FirstOrDefault(x => x.IsTrial);
            if (alreadyActivated != null)
            {
                if (alreadyActivated.EndDate < DateTime.Today)
                {
                    throw new FeatureModuleException(
                        $"Kan ikke aktivere prøveperiode. Har allerede været aktiveret fra {alreadyActivated.StartDate.ToDanishDate()} - {alreadyActivated.EndDate.ToDanishDate()} ");
                }

                throw new FeatureModuleException(
                    $"Kan ikke aktivere prøveperiode. Blev aktiveret den {alreadyActivated.StartDate.ToDanishDate()}, og er aktiv til {alreadyActivated.EndDate.ToDanishDate()} ");
            }

            var featureModuleTenantEntry = _featureModuleFactory.CreateTrialEntry(featureModule);
            return featureModuleTenantEntry;
        }

        public async Task<FeatureModuleTenantEntry> ActivateModule(Guid tenantId, Guid userId, FeatureModule featureModule, List<FeatureModuleTenantEntry> featureModuleTenantEntries, int unitsCount)
        {
            var latestModule = featureModuleTenantEntries.MaxBy(x => x.CreateDate);
            if (latestModule != null && !latestModule.IsTrial && latestModule.ActivationState == ActivationState.Active)
            {
                if (latestModule.EndDate > DateTime.Today)
                {
                    throw new FeatureModuleException(
                        $"Kan ikke aktivere modul. Modulet er allerede aktiveret fra {latestModule.ActivationDate.ToDanishDate()} - {latestModule.EndDate.ToDanishDate()}. Fornyelse sker automatisk");
                }
            }
            var trialModuleEntry = featureModuleTenantEntries.FirstOrDefault(x => x.IsTrial);
            var hasTrialBeenActivated = trialModuleEntry != null;
            decimal price;

            if (featureModule.FixedPrice != null)
            {
                price = featureModule.FixedPrice.Value;
            }
            else
            {
                price = featureModule.MinimumPrice + (featureModule.UnitPrice * (unitsCount - featureModule.UnitsIncluded > 0 ? unitsCount - featureModule.UnitsIncluded : 0));
                if (price > featureModule.MaximumPrice)
                {
                    price = featureModule.MaximumPrice.Value;
                }
            }

            decimal discountOnce = 0;
            bool isEarlyBirdApplied = false;
            if (!hasTrialBeenActivated)
            {
                discountOnce = featureModule.EarlyBirdDiscount;
                isEarlyBirdApplied = true;

            }
            else if (latestModule?.ActivationState == ActivationState.Active)
            {
                var daysTillTrialEnd = DateTime.Today.GetDaysDiff(trialModuleEntry.EndDate);
                discountOnce = Math.Round(daysTillTrialEnd > 0 ? (price * 12 / 365) : 0, 2) * daysTillTrialEnd;
            }

            FeatureModuleTenantEntry featureModuleTenantEntry;
            var latestActiveModule = featureModuleTenantEntries.Where(x => x.ActivationState == ActivationState.Active && !x.IsTrial && x.ActivationDate <= DateTime.Today && x.EndDate >= DateTime.Today).MaxBy(x => x.CreateDate);
            if (latestActiveModule == null)
            {
                var startDate = DateTime.Today.GetStartDayForQuarter();
                if (DateTime.Today > DateTime.Today.GetStartDayForQuarter())
                {
                    //use next quarter
                    startDate = DateTime.Today.GetEndDayForQuarter().AddDays(1);
                }

                featureModuleTenantEntry = _featureModuleFactory.CreateActivationEntry(featureModule, startDate, price, discountOnce, isEarlyBirdApplied, unitsCount);
            }
            else
            {
                featureModuleTenantEntry = _featureModuleFactory.CreateReActivationEntry(latestActiveModule);
            }

            return featureModuleTenantEntry;
        }

        public async Task<FeatureModuleTenantEntry> DeactivateModule(Guid tenantId, Guid userId, FeatureModule featureModule, List<FeatureModuleTenantEntry> featureModuleTenantEntries)
        {
            var latestModule = featureModuleTenantEntries.MaxBy(x => x.CreateDate);
            if (latestModule != null)
            {
                if (latestModule.ActivationState == ActivationState.Deactivated)
                {
                    throw new FeatureModuleException(
                        $"Kan ikke deaktivere modul. Modulet er allerede deaktiveret");
                }

                var featureModuleTenantEntry = _featureModuleFactory.CreateDeactivationEntry(featureModule);
                featureModuleTenantEntry.EndDate = new DateTime(2050, 1, 1);
                if (latestModule.IsTrial)
                {
                    featureModuleTenantEntry.HandlingState = HandlingState.NeedsNoHandling;
                }
                else
                {
                    featureModuleTenantEntry.HandlingState = HandlingState.NeedsHandling;
                }
                return featureModuleTenantEntry;
            }

            throw new FeatureModuleException(
                $"Kan ikke deaktivere modul. Modulet har aldrig været aktiveret");
        }

        public async Task<FeatureModuleTenantEntry> Renew(Guid tenantId, Guid userId, FeatureModuleTenantEntry featureModuleTenantEntry)
        {

            if (featureModuleTenantEntry.ActivationState == ActivationState.Deactivated)
            {
                throw new FeatureModuleException(
                    $"Kan ikke forny modul. Modulet er deaktiveret");
            }

            if (featureModuleTenantEntry.ActivationState == ActivationState.Active && featureModuleTenantEntry.EndDate > DateTime.Today)
            {
                throw new FeatureModuleException(
                    $"Kan ikke forny modul. Modulet er aktiveret indtil {featureModuleTenantEntry.EndDate.ToDanishDate()}");
            }

            featureModuleTenantEntry.HandlingState = HandlingState.NeedsNoHandling;
            var renewedFeatureModuleTenantEntry = _featureModuleFactory.CreateRenewEntry(featureModuleTenantEntry);
            return renewedFeatureModuleTenantEntry;

        }
    }
}