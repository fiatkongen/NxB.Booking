using Munk.AspNetCore;
using NxB.BookingApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore.Sql;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Infrastructure
{
    public class FeatureModuleRepository : BaseRepository<AppDbContext>, IFeatureModuleRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly IClaimsProvider _claimsProvider;

        public FeatureModuleRepository(AppDbContext appDbContext, IClaimsProvider claimsProvider) : base(appDbContext)
        {
            _appDbContext = appDbContext;
            _claimsProvider = claimsProvider;
        }

        public void Add(FeatureModule featureModule)
        {
            this._appDbContext.FeatureModules.Add(featureModule);
        }

        public void Update(FeatureModule featureModule)
        {
            this._appDbContext.FeatureModules.Update(featureModule);
        }

        public void Update(FeatureModuleTenantEntry featureModuleEntry)
        {
            this._appDbContext.FeatureModuleTenantEntries.Update(featureModuleEntry);
        }

        public async Task MarkAsDeleted(Guid id)
        {
            var featureModule = await FindSingleOrDefault(id);
            featureModule.IsDeleted = true;
        }

        public async Task MarkAsUndeleted(Guid id)
        {
            var featureModule = await FindSingleOrDefault(id);
            featureModule.IsDeleted = false;
        }

        public async Task<List<FeatureModule>> FindAll(bool includeDeleted)
        {
            var featureModules = await this._appDbContext.FeatureModules.Where(x => !x.IsDeleted || includeDeleted).ToListAsync();
            return featureModules;
        }

        public async Task<FeatureModule> FindSingleOrDefault(Guid id)
        {
            var featureModule = this._appDbContext.FeatureModules.SingleOrDefault(x => x.Id == id);
            return featureModule;
        }

        public async Task<FeatureModuleTenantEntry> FindSingleOrDefaultEntry(Guid id)
        {
            var featureModuleEntry = await AppDbContext.FeatureModuleTenantEntries.FirstOrDefaultAsync(x => x.Id == id);
            return featureModuleEntry;
        }

        public async Task<FeatureModuleTenantEntry> FindLastFeatureModuleEntryForFeatureModule(Guid featureModuleId)
        {
            var featureModuleEntry = await GetTenantFilteredEntitiesQuery<FeatureModuleTenantEntry>(_claimsProvider.GetTenantId()).Where(x => x.FeatureModuleId == featureModuleId).OrderByDescending(x => x.CreateDate).FirstOrDefaultAsync();
            return featureModuleEntry;
        }

        public Task<FeatureModuleTenantEntry> FindLastFeatureModuleEntryForFeatureModuleFromTenantId(Guid featureModuleId, Guid tenantId)
        {
            return AppDbContext.FeatureModuleTenantEntries.Where(x => x.FeatureModuleId == featureModuleId && x.TenantId == tenantId).OrderByDescending(x => x.CreateDate).FirstOrDefaultAsync();
        }

        public async Task<List<FeatureModuleTenantEntry>> FindAllFeatureModuleEntriesForFeatureModule(Guid featureModuleId)
        {
            var featureModuleEntries = await GetTenantFilteredEntitiesQuery<FeatureModuleTenantEntry>(_claimsProvider.GetTenantId()).Where(x => x.FeatureModuleId == featureModuleId).OrderByDescending(x => x.CreateDate).ToListAsync();
            return featureModuleEntries;
        }

        public async Task<List<FeatureModuleTenantEntry>> FindAllFeatureModuleEntries()
        {
            var featureModuleEntries = await GetTenantFilteredEntitiesQuery<FeatureModuleTenantEntry>(_claimsProvider.GetTenantId()).OrderByDescending(x => x.CreateDate).ToListAsync();
            return featureModuleEntries;
        }

        public async Task<List<FeatureModuleTenantEntry>> FindAllFeatureModuleEntriesForAllTenants()
        {
            var featureModuleEntries = await AppDbContext.FeatureModuleTenantEntries.OrderByDescending(x => x.CreateDate).ToListAsync();
            return featureModuleEntries;
        }

        public void Add(FeatureModuleTenantEntry featureModuleTenantEntry)
        {
            this._appDbContext.FeatureModuleTenantEntries.Add(featureModuleTenantEntry);
        }
    }
}