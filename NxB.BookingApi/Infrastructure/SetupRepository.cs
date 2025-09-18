using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class SetupRepository : ISetupRepository
    {
        private readonly AppTallyDbContext _appTallyDbContext;
        private readonly SetupPeriodFactory _setupPeriodFactory;
        private readonly SetupAccessFactory _setupAccessFactory;
        private readonly ITConMasterRadioTenantMapRepository _tconMasterRadioTenantMapRepository;
        private IQueryable<TConTBDSetupPeriod> _tconTenantFilteredSetupPeriods => _appTallyDbContext.TConTBDSetupPeriods.Where(x => x._MasterAddr == _tconMasterRadioTenantMapRepository.MasterRadioId).OrderBy(x => x._No);
        private IQueryable<TConTBDSetupAccess> _tconTenantFilteredSetupAccesses => _appTallyDbContext.TConTBDSetupAccesses.Where(x => x._MasterAddr == _tconMasterRadioTenantMapRepository.MasterRadioId).OrderBy(x => x._No);

        public SetupRepository(AppTallyDbContext appTallyDbContext, SetupPeriodFactory setupPeriodFactory, ITConMasterRadioTenantMapRepository tconMasterRadioTenantMapRepository, SetupAccessFactory setupAccessFactory)
        {
            this._appTallyDbContext = appTallyDbContext;
            _setupPeriodFactory = setupPeriodFactory;
            _tconMasterRadioTenantMapRepository = tconMasterRadioTenantMapRepository;
            _setupAccessFactory = setupAccessFactory;
        }

        public async Task<List<SetupPeriod>> FindSetupPeriods()
        {
            var tconTbdSetupPeriods = await _tconTenantFilteredSetupPeriods.ToListAsync();
            var setupPeriods = tconTbdSetupPeriods.Select(x => _setupPeriodFactory.Create(x)).ToList();
            return setupPeriods;
        }

        public async Task Update(SetupPeriod setupPeriod)
        {
            _appTallyDbContext.Update(setupPeriod.GetTConEntity());
            await _appTallyDbContext.SaveChangesAsync();
        }

        public async Task Add(SetupPeriod setupPeriod)
        {
            await _appTallyDbContext.AddAsync(setupPeriod.GetTConEntity());
            await _appTallyDbContext.SaveChangesAsync();
        }

        public async Task RemoveSetupPeriod(int no)
        {
            var setupPeriod = await _tconTenantFilteredSetupPeriods.SingleAsync(x => x._No == no);
            _appTallyDbContext.Remove(setupPeriod);
            await _appTallyDbContext.SaveChangesAsync();
        }

        public async Task<List<SetupAccess>> FindSetupAccesses()
        {
            var conTbdSetupAccesses = await _tconTenantFilteredSetupAccesses.ToListAsync();
            var setupAccesses = conTbdSetupAccesses.Select(x => _setupAccessFactory.Create(x)).ToList();
            return setupAccesses;
        }

        public async Task Update(SetupAccess setupAccess)
        {
            _appTallyDbContext.Update(setupAccess.GetTConEntity());
            await _appTallyDbContext.SaveChangesAsync();
        }

        public async Task Add(SetupAccess setupAccess)
        {
            await _appTallyDbContext.AddAsync(setupAccess.GetTConEntity());
            await _appTallyDbContext.SaveChangesAsync();
        }

        public async Task RemoveSetupAccess(int no)
        {
            var setupAccess = await _tconTenantFilteredSetupAccesses.SingleAsync(x => x._No == no);
            _appTallyDbContext.Remove(setupAccess);
            await _appTallyDbContext.SaveChangesAsync();
        }
    }
}