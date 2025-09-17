using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public interface IFeatureModuleRepository
    {
        void Add(FeatureModule featureModule);
        void Update(FeatureModule featureModule);
        void Update(FeatureModuleTenantEntry featureModuleEntry);
        Task MarkAsDeleted(Guid id);
        Task MarkAsUndeleted(Guid id);
        Task<List<FeatureModule>> FindAll(bool includeDeleted);
        Task<FeatureModule> FindSingleOrDefault(Guid id);
        Task<FeatureModuleTenantEntry> FindSingleOrDefaultEntry(Guid id);
        Task<FeatureModuleTenantEntry> FindLastFeatureModuleEntryForFeatureModule(Guid featureModuleId);
        Task<FeatureModuleTenantEntry> FindLastFeatureModuleEntryForFeatureModuleFromTenantId(Guid featureModuleId, Guid tenantId);
        Task<List<FeatureModuleTenantEntry>> FindAllFeatureModuleEntriesForFeatureModule(Guid featureModuleId);
        Task<List<FeatureModuleTenantEntry>> FindAllFeatureModuleEntries();
        Task<List<FeatureModuleTenantEntry>> FindAllFeatureModuleEntriesForAllTenants();
        void Add(FeatureModuleTenantEntry featureModuleTenantEntry);
    }
}