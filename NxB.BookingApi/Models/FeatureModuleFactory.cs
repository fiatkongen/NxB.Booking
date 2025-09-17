using NxB.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NxB.Dto.TenantApi;
using NxB.BookingApi.Exceptions;

namespace NxB.BookingApi.Models
{
    public class FeatureModuleFactory
    {
        private readonly IClaimsProvider _claimsProvider;

        public FeatureModuleFactory(IClaimsProvider claimsProvider)
        {
            _claimsProvider = claimsProvider;
        }

        public FeatureModule Create(string name)
        {
            return new FeatureModule
            {
                Id = Guid.NewGuid(),
                Name = name,
                TrialDays = 30,
            };
        }

        public FeatureModuleTenantEntry CreateTrialEntry(FeatureModule featureModule)
        {
            if (featureModule.TrialDays == null) throw new FeatureModuleException();
            return new FeatureModuleTenantEntry
            {
                Id = Guid.NewGuid(),
                ActivationDate = DateTime.Today,
                TenantId = _claimsProvider.GetTenantId(),
                UserId = _claimsProvider.GetUserId(),
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(featureModule.TrialDays.Value),
                IsTrial = true,
                FeatureModuleId = featureModule.Id,
                MonthlyPrice = 0,
                ActivationState = ActivationState.Active,
                HandlingState = HandlingState.NeedsHandlingOnExpiration
            };
        }

        public FeatureModuleTenantEntry CreateActivationEntry(FeatureModule featureModule, DateTime startDate, decimal price, decimal discountAmountOnce, bool isEarlyBirdApplied, int unitsCount)
        {
            return new FeatureModuleTenantEntry
            {
                Id = Guid.NewGuid(),
                ActivationDate = DateTime.Today,
                TenantId = _claimsProvider.GetTenantId(),
                UserId = _claimsProvider.GetUserId(),
                StartDate = startDate,
                EndDate = startDate.AddYears(1),
                IsTrial = false,
                IsEarlyBirdApplied = isEarlyBirdApplied,
                FeatureModuleId = featureModule.Id,
                MonthlyPrice = price,
                ActivationState = ActivationState.Active,
                HandlingState = HandlingState.NeedsHandling,
                DiscountAmountOnce = discountAmountOnce,
                UnitsCount = unitsCount,
                InitialQuarterDays = DateTime.Today.GetDaysDiff(DateTime.Today.GetEndDayForQuarter())
            };
        }

        public FeatureModuleTenantEntry CreateReActivationEntry(FeatureModuleTenantEntry reActivateEntry)
        {
            return new FeatureModuleTenantEntry
            {
                Id = Guid.NewGuid(),
                ActivationDate = reActivateEntry.ActivationDate,
                TenantId = _claimsProvider.GetTenantId(),
                UserId = _claimsProvider.GetUserId(),
                StartDate = reActivateEntry.StartDate,
                EndDate = reActivateEntry.EndDate,
                IsTrial = reActivateEntry.IsTrial,
                FeatureModuleId = reActivateEntry.FeatureModuleId,
                MonthlyPrice = reActivateEntry.MonthlyPrice,
                ActivationState = ActivationState.Active,
                HandlingState = HandlingState.NeedsHandling,
                DiscountPercentPermanently = reActivateEntry.DiscountPercentPermanently,
                DiscountAmountOnce = reActivateEntry.DiscountAmountOnce,
                DiscountAmountPermanently = reActivateEntry.DiscountAmountPermanently,
                DiscountPercentOnce = reActivateEntry.DiscountPercentOnce,
                InitialQuarterDays = reActivateEntry.InitialQuarterDays,
                UnitsCount = reActivateEntry.UnitsCount,
            };
        }

        public FeatureModuleTenantEntry CreateDeactivationEntry(FeatureModule featureModule)
        {
            return new FeatureModuleTenantEntry
            {
                Id = Guid.NewGuid(),
                TenantId = _claimsProvider.GetTenantId(),
                UserId = _claimsProvider.GetUserId(),
                StartDate = DateTime.Today,
                EndDate = DateTime.Today,
                IsTrial = false,
                FeatureModuleId = featureModule.Id,
                MonthlyPrice = 0,
                ActivationState = ActivationState.Deactivated,
                ActivationDate = DateTime.Today,
            };
        }

        public FeatureModuleTenantEntry CreateRenewEntry(FeatureModuleTenantEntry renewEntry)
        {
            return new FeatureModuleTenantEntry
            {
                Id = Guid.NewGuid(),
                TenantId = _claimsProvider.GetTenantId(),
                UserId = _claimsProvider.GetUserId(),
                StartDate = renewEntry.StartDate.AddYears(1),
                EndDate = renewEntry.EndDate.AddYears(1),
                IsTrial = false,
                FeatureModuleId = renewEntry.FeatureModuleId,
                MonthlyPrice = renewEntry.MonthlyPrice,
                ActivationState = ActivationState.Active,
                HandlingState = HandlingState.NeedsHandlingOnExpiration,
                DiscountPercentPermanently = renewEntry.DiscountPercentPermanently,
                DiscountAmountPermanently = renewEntry.DiscountAmountPermanently,
            };
        }
    }
}