using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    public interface IFeatureModuleService
    {
        Task<FeatureModuleTenantEntry> ActivateModuleForTrial(Guid tenantId, Guid userId, FeatureModule featureModule, List<FeatureModuleTenantEntry> featureModuleTenantEntries);
        Task<FeatureModuleTenantEntry> ActivateModule(Guid tenantId, Guid userId, FeatureModule featureModule,
            List<FeatureModuleTenantEntry> featureModuleTenantEntries, int unitsCount);
        Task<FeatureModuleTenantEntry> DeactivateModule(Guid tenantId, Guid userId, FeatureModule featureModule,
            List<FeatureModuleTenantEntry> featureModuleTenantEntries);
        Task<FeatureModuleTenantEntry> Renew(Guid tenantId, Guid userId,
            FeatureModuleTenantEntry featureModuleTenantEntry);
    }
}