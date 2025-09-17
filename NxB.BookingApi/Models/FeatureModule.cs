using NxB.Dto.TenantApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    public class FeatureModule
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public string Name { get; set; }
        public decimal? FixedPrice { get; set; }
        public decimal UnitPrice { get; set; }
        public int UnitsIncluded { get; set; }
        public decimal MinimumPrice { get; set; }
        public decimal? MaximumPrice { get; set; }
        public int? TrialDays { get; set; }
        public decimal EarlyBirdDiscount { get; set; }
        public string UrlMatch { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class FeatureModuleTenantEntry : ITenantEntity
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now.ToEuTimeZone();
        public Guid FeatureModuleId { get; set; }
        public Guid TenantId { get; set; }
        public ActivationState ActivationState { get; set; }
        public decimal MonthlyPrice { get; set; }
        public DateTime ActivationDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid UserId { get; set; }
        public decimal DiscountPercentPermanently { get; set; }
        public decimal DiscountAmountPermanently { get; set; }
        public decimal DiscountPercentOnce { get; set; }
        public decimal DiscountAmountOnce { get; set; }
        public decimal InitialQuarterDays { get; set; }
        public bool IsTrial { get; set; }
        public bool IsEarlyBirdApplied { get; set; }
        public HandlingState HandlingState { get; set; }
        public int UnitsCount { get; set; }

        public decimal InitialQuarterCost => InitialQuarterDays * DailyPrice;
        public decimal YearlyPrice => (MonthlyPrice * 12);
        public decimal YearlyDiscountedPrice => YearlyPrice
                                                - TotalDiscount;

        public decimal DailyPrice => Math.Round(YearlyDiscountedPrice > 0 ? YearlyDiscountedPrice / 365 : 0, 2);
        public decimal Total => InitialQuarterCost + YearlyDiscountedPrice;
        public decimal TotalDiscount => DiscountAmountOnce + (MonthlyPrice * 12 * (DiscountPercentOnce * 0.01m))
                                                            + DiscountAmountPermanently + (MonthlyPrice * 12 * (DiscountPercentPermanently * 0.01m));

        public bool NeedsHandling => HandlingState == HandlingState.NeedsHandling
                                     || (HandlingState == HandlingState.NeedsHandlingOnExpiration && EndDate < DateTime.Today);

        //public bool IsActive => ActivationState == ActivationState.Active && DateTime.Today >= ActivationDate && DateTime.Today <= EndDate;
        public bool IsActive => ActivationState == ActivationState.Active;
    }
}