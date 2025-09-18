using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Munk.Utils.Object;

namespace NxB.Dto.TenantApi
{
    public class CreateFeatureModuleDto
    {
        public string Text { get; set; }

        [NoEmpty]
        public string Name { get; set; }
        public decimal? FixedPrice { get; set; }
        public decimal UnitPrice { get; set; }
        public int UnitsIncluded { get; set; }
        public decimal MinimumPrice { get; set; }
        public decimal? MaximumPrice { get; set; }
        public int? TrialDays { get; set; }
        public decimal EarlyBirdDiscount { get; set; }
        public string UrlMatch { get; set; }
    }

    public class FeatureModuleDto : CreateFeatureModuleDto
    {
        [NoEmpty]
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class ModifyFeatureModuleDto : FeatureModuleDto
    {

    }

    public class FeatureModuleTenantEntryDto
    {
        public Guid Id { get; set; }
        public Guid FeatureModuleId { get; set; }
        public Guid TenantId { get; set; }
        public string TenantName { get; set; }
        public DateTime CreateDate { get; set; }
        public ActivationState ActivationState { get; set; }
        public decimal MonthlyPrice { get; set; }
        public DateTime ActivationDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid UserId { get; set; }
        public bool NeedsHandling { get; set; }
        public bool IsTrial { get; set; }
        public bool IsEarlyBirdApplied { get; set; }
        public decimal DiscountPercentPermanently { get; set; }
        public decimal DiscountAmountPermanently { get; set; }
        public decimal DiscountPercentOnce { get; set; }
        public decimal DiscountAmountOnce { get; set; }
        public decimal InitialQuarterCost { get; set; }
        public decimal InitialQuarterDays { get; set; }
        public HandlingState HandlingState { get; set; }
        public int UnitsCount { get; set; }
        public decimal YearlyDiscountedPrice { get; set; }
        public decimal YearlyPrice { get; set; }
        public decimal DailyPrice { get; set; }
        public decimal Total { get; set; }
        public decimal TotalDiscount { get; set; }
        public bool IsActiveEntry { get; set; }
    }

    public class ModifyAdminFeatureModuleTenantEntryDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public decimal DiscountPercentPermanently { get; set; }
        public decimal DiscountAmountPermanently { get; set; }
        public decimal DiscountPercentOnce { get; set; }
        public decimal DiscountAmountOnce { get; set; }
        public HandlingState HandlingState { get; set; }
    }

    public class ActivateFeatureModuleDto
    {
        public Guid FeatureModuleId { get; set; }
        public Guid TenantId { get; set; }

    }

    public enum ActivationState
    {
        None = 0,
        Active = 1,
        Deactivated = 2,
    }

    public enum HandlingState
    {
        None,
        NeedsHandling,
        NeedsHandlingOnExpiration,
        NeedsNoHandling
    }
}
