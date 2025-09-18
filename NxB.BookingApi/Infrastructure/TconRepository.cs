using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class TConRepository : ITConRepository
    {
        private readonly AppTallyDbContext _tallyDbContext;
        private Dictionary<int, TConRadioType> _cachedRadioTypes = new();
        private readonly IMasterRadioIdProvider _masterRadioIdProvider;
        private int _masterRadioId => _masterRadioIdProvider.MasterRadioId;

        public TConRepository(AppTallyDbContext tallyDbContext, IMasterRadioIdProvider masterRadioIdProvider)
        {
            _tallyDbContext = tallyDbContext;
            _masterRadioIdProvider = masterRadioIdProvider;
        }

        private IQueryable<TConMasterRadio> TConTenantFilteredMasterRadios => _tallyDbContext.TConMasterRadios.Where(x => x.MasterAddr == _masterRadioId);
        private IQueryable<TConRadio> TConTenantFilteredRadios => _tallyDbContext.TConRadios.Where(x => x.MasterAddr == _masterRadioId);
        private IQueryable<TConRadioAccessCode> TConTenantFilteredRadioAccessCodes => _tallyDbContext.TConRadioAccessCodes.Join(_tallyDbContext.TConRadios, rac => rac._RadioAddr, r => r.RadioAddr, (rac, r) => new { radioAccessCode = rac, radio = r }).Where(result => result.radio.MasterAddr == _masterRadioId).Select(result => result.radioAccessCode);
        private IQueryable<TConSocketTBB> TConTenantFilteredTBBSockets => _tallyDbContext.TConTBBSockets.Join(_tallyDbContext.TConRadios, s => s.RadioAddr, r => r.RadioAddr, (s, r) => new { socket = s, radio = r }).Where(result => result.radio.MasterAddr == _masterRadioId).Select(result => result.socket);
        private IQueryable<TConSocketTWC> TConTenantFilteredTWCSockets => _tallyDbContext.TConTWCSockets.Join(_tallyDbContext.TConRadios, s => s.RadioAddr, r => r.RadioAddr, (s, r) => new { socket = s, radio = r }).Where(result => result.radio.MasterAddr == _masterRadioId).Select(result => result.socket);
        private IQueryable<TConStatusTBD> TConTenantFilteredTDBStatus => _tallyDbContext.TConTBDStatuses.Join(_tallyDbContext.TConRadios, s => s.RadioAddr, r => r.RadioAddr, (s, r) => new { status = s, radio = r }).Where(result => result.radio.MasterAddr == _masterRadioId).Select(result => result.status);
        private IQueryable<TConTBBConsumption> TConTenantFilteredTBBConsumptions => _tallyDbContext.TConTBBConsumptions.Join(_tallyDbContext.TConRadios, s => s.RadioAddr, r => r.RadioAddr, (c, r) => new { consumption = c, radio = r }).Where(result => result.radio.MasterAddr == _masterRadioId).Select(result => result.consumption);
        private IQueryable<TConTWCConsumption> TConTenantFilteredTBEConsumptions => _tallyDbContext.TConTBE8Consumptions.Join(_tallyDbContext.TConRadios, s => s.RadioAddr, r => r.RadioAddr, (c, r) => new { consumption = c, radio = r }).Where(result => result.radio.MasterAddr == _masterRadioId).Select(result => result.consumption);
        private IQueryable<TConTBDAccessLog> TConTenantFilteredTBDAccessLogs => _tallyDbContext.TConTBDAccessLogs.Join(_tallyDbContext.TConRadios, s => s.RadioAddr, r => r.RadioAddr, (a, r) => new { accessLog = a, radio = r }).Where(result => result.radio.MasterAddr == _masterRadioId).Select(result => result.accessLog);
        private IQueryable<TConSocketTWEV> TConTenantFilteredTWEVSockets => _tallyDbContext.TConTWEVSockets.Join(_tallyDbContext.TConRadios, s => s.RadioAddr, r => r.RadioAddr, (s, r) => new { socket = s, radio = r }).Where(result => result.radio.MasterAddr == _masterRadioId).Select(result => result.socket);
        private IQueryable<TConTWIStatus> TConTenantFilteredTWIControllers => _tallyDbContext.TConTWIStatuses.Join(_tallyDbContext.TConRadios, s => s.RadioAddr, r => r.RadioAddr, (c, r) => new { controller = c, radio = r }).Where(result => result.radio.MasterAddr == _masterRadioId).Select(result => result.controller);

        public ITConRepository CloneWithCustomClaimsProvider(IClaimsProvider overrideClaimsProvider)
        {
            return new TConRepository(_tallyDbContext, _masterRadioIdProvider.CloneWithCustomClaimsProvider(overrideClaimsProvider));
        }

        public static async Task<List<TConTBDAccessLogExtended>> FindTConTenantFilteredTBDAccessLogsExtended(AppTallyDbContext tallyDbContext, DateTime from)
        {
            var logs = await tallyDbContext.TConTBDAccessLogs.Join(tallyDbContext.TConRadios, s => s.RadioAddr,
                r => r.RadioAddr,
                (a, r) => new { accessLog = a, radio = r }).Where(x => x.accessLog.SavedDateTime >= from).ToListAsync();

            var logsExtended = logs.Select(result => new TConTBDAccessLogExtended(result.accessLog, result.radio.MasterAddr)).ToList();
            return logsExtended;
        }


        public void AddTConRadio(TConRadio radio)
        {
            this._tallyDbContext.TConRadios.Add(radio);
        }

        public Task<List<TConMasterRadio>> FindAllTConMasterRadios()
        {
            return this.TConTenantFilteredMasterRadios.ToListAsync();
        }

        public Task<List<TConRadio>> FindAllRadios()
        {
            return TConTenantFilteredRadios.ToListAsync();
        }

        public Task<List<TConRadio>> FindAllTConTBBRadios()
        {
            return TConTenantFilteredRadios.Where(x => x.Type == TConRadioType.TBB).ToListAsync();
        }

        public async Task<List<TConRadio>> FindAllTConTBDRadios()
        {
            var twiRadiosIds = await TConTenantFilteredTWIControllers.Where(x => x.Modus == (byte)TWIModus.DoorOrTimer || x.Modus == (byte)TWIModus.GateIn || x.Modus == (byte)TWIModus.GateOut).Select(x => x.RadioAddr).ToListAsync();
            var tdbRadios = await TConTenantFilteredRadios.Where(x => x.Type == TConRadioType.TBD || twiRadiosIds.Contains(x.RadioAddr)).ToListAsync();
            return tdbRadios;
        }

        public Task<List<TConRadio>> FindAllTConTBERadios()
        {
            return TConTenantFilteredRadios.Where(x => x.Type == TConRadioType.TBE).ToListAsync();
        }

        public Task<List<TConRadio>> FindAllTConTWIRadios()
        {
            return TConTenantFilteredRadios.Where(x => x.Type == TConRadioType.TWI).ToListAsync();
        }

        public Task<List<TConRadio>> FindAllTConTWEVRadios()
        {
            return TConTenantFilteredRadios.Where(x => x.Type == TConRadioType.TWEV).ToListAsync();
        }

        public Task<TConRadio> FindSingleTConTBBRadio(int radioAddress)
        {
            return _tallyDbContext.TConRadios.SingleAsync(x => x.RadioAddr == radioAddress && x.Type == TConRadioType.TBB);
        }

        public Task<TConRadio> FindSingleTConTBDRadio(int radioAddress)
        {
            return _tallyDbContext.TConRadios.SingleAsync(x => x.RadioAddr == radioAddress && x.Type == TConRadioType.TBD);
        }

        public Task<TConRadio> FindSingleTConTBDOrTWIRadio(int radioAddress)
        {
            return _tallyDbContext.TConRadios.SingleAsync(x => x.RadioAddr == radioAddress && (x.Type == TConRadioType.TBD || x.Type == TConRadioType.TWI));
        }

        public Task<TConRadio> FindSingleTConTWCRadio(int radioAddress)
        {
            return _tallyDbContext.TConRadios.SingleAsync(x => x.RadioAddr == radioAddress && x.Type == TConRadioType.TBE);
        }

        public Task<TConRadio> FindSingleTConTWEVRadio(int radioAddress)
        {
            return _tallyDbContext.TConRadios.SingleAsync(x => x.RadioAddr == radioAddress && x.Type == TConRadioType.TWEV);
        }

        public Task<TConRadio> FindSingleTConTWIRadio(int radioAddress)
        {
            return _tallyDbContext.TConRadios.SingleAsync(x => x.RadioAddr == radioAddress && x.Type == TConRadioType.TWI);
        }

        public Task<TConRadio> FindSingleRadio(int radioAddress)
        {
            return _tallyDbContext.TConRadios.SingleAsync(x => x.RadioAddr == radioAddress);
        }

        public async Task<TConRadioType> GetRadioType(int radioAddress)
        {
            if (_cachedRadioTypes.Count == 0)
            {
                this.CacheRadioTypes(await FindAllRadios());
            }

            return this._cachedRadioTypes[radioAddress];
        }

        public async Task<List<ITConSocket>> FindAllTConSockets()
        {
            var tbbSockets = await TConTenantFilteredTBBSockets.Cast<ITConSocket>().ToListAsync();
            var tbeSockets = await TConTenantFilteredTWCSockets.Cast<ITConSocket>().ToListAsync();
            return tbbSockets.Concat(tbeSockets).ToList();
        }

        public Task<List<TConSocketTBB>> FindAllTConTBBSockets()
        {
            return TConTenantFilteredTBBSockets.ToListAsync();
        }

        public Task<List<TConSocketTWC>> FindAllTConTWCSockets()
        {
            return TConTenantFilteredTWCSockets.ToListAsync();
        }

        public Task<List<TConSocketTWEV>> FindAllTConTWEVSockets()
        {
            return TConTenantFilteredTWEVSockets.ToListAsync();
        }

        public Task<TConSocketTBB> FindSingleTConTBBSocket(int radioAddress, int socketNo)
        {
            return _tallyDbContext.TConTBBSockets.SingleAsync(x => x.RadioAddr == radioAddress && x.SocketNo == socketNo);
        }

        public Task<TConSocketTWC> FindSingleTConTWCSocket(int radioAddress, int socketNo)
        {
            return _tallyDbContext.TConTWCSockets.SingleAsync(x => x.RadioAddr == radioAddress && x.SocketNo == socketNo);
        }

        public Task<TConSocketTWEV> FindSingleTConTWEVSocket(int radioAddress, int socketNo)
        {
            return _tallyDbContext.TConTWEVSockets.SingleAsync(x => x.RadioAddr == radioAddress && x.SocketNo == socketNo);
        }

        public Task<TConSocketTBB> FindSingleOrDefaultTConTBBSocket(int radioAddress, int socketNo)
        {
            return _tallyDbContext.TConTBBSockets.SingleOrDefaultAsync(x => x.RadioAddr == radioAddress && x.SocketNo == socketNo);
        }

        public Task<TConSocketTWC> FindSingleOrDefaultTConTWCSocket(int radioAddress, int socketNo)
        {
            return _tallyDbContext.TConTWCSockets.SingleOrDefaultAsync(x => x.RadioAddr == radioAddress && x.SocketNo == socketNo);
        }

        public Task<List<TConTWCConsumption>> FindTConTBESocketConsumptions(int radioAddress, int socketNo)
        {
            return _tallyDbContext.TConTBE8Consumptions.Where(x => x.RadioAddr == radioAddress && x.SocketNo == socketNo).ToListAsync();
        }

        public Task<List<TConTBBConsumption>> FindTConTBBSocketConsumptions(int radioAddress, int socketNo)
        {
            return _tallyDbContext.TConTBBConsumptions.Where(x => x.RadioAddr == radioAddress && x.SocketNo == socketNo).ToListAsync();
        }

        public Task<List<TConTWEVConsumption>> FindTConTWECSocketConsumptions(int radioAddress, int socketNo)
        {
            return _tallyDbContext.TConTWEVConsumptions.Where(x => x.RadioAddr == radioAddress && x.SocketNo == socketNo).ToListAsync();
        }

        public void AddTConRadioAccessCode(TConRadioAccessCode tconRadioAccessCode)
        {
            this._tallyDbContext.TConRadioAccessCodes.Add(tconRadioAccessCode);
        }

        public async Task<int> RemoveTConRadioAccessWithCode(int code)
        {
            var accessCodes = await TConTenantFilteredRadioAccessCodes.Where(x => x._Code == code).ToListAsync();
            foreach (var accessCode in accessCodes)
            {
                await RemoveAccessFromRadio(accessCode);
            }

            return accessCodes.Count;
        }

        private async Task RemoveAccessFromRadio(TConRadioAccessCode accessCode)
        {
            var radio = await this.FindSingleRadio(accessCode._RadioAddr);
            if (radio.OnLine)
            {
                accessCode.Remove();
            }
            else
            {
                // An accesscode with this status means that the radio was offline when the access was added,
                // and has not been online - hence the status NOT active AND new = 1
                if (accessCode._Active && accessCode._New == 1)
                {
                    _tallyDbContext.Remove(accessCode);
                }
                // An accesscode with this status means that the radio was online when the access was added,
                // and HAS not been online - hence the status Active and new = 0
                // this poses a dilemma. Remove the code and the Radio will NOT be updated when brought online.
                // OR do not remove the code and if the code/card will not be released before the radio is brought back online
                else if (accessCode._Active && accessCode._New == 0)
                {
                    _tallyDbContext.Remove(accessCode);
                }
            }
        }

        public async Task RemoveTConRadioAccessCodeFromSingleRadio(int radioAddress, int code)
        {
            var accessCode = await TConTenantFilteredRadioAccessCodes.SingleOrDefaultAsync(x => x._Code == code && x._RadioAddr == radioAddress);
            if (accessCode != null)
            {
                await RemoveAccessFromRadio(accessCode);
            }
        }

        public Task<TConRadioAccessCode> FindTConRadioAccessCode(int radioAddress, int code)
        {
            return _tallyDbContext.TConRadioAccessCodes.SingleOrDefaultAsync(x => x._Code == code && x._RadioAddr == radioAddress);
        }

        public Task<List<TConRadioAccessCode>> FindAllTConRadioAccessCodes()
        {
            return TConTenantFilteredRadioAccessCodes.ToListAsync();
        }

        public Task<List<TConRadioAccessCode>> FindTConRadioAccessCodesWithCode(int code)
        {
            return TConTenantFilteredRadioAccessCodes.Where(x => x._Code == code).ToListAsync();
        }

        public Task<List<TConRadioAccessCode>> FindTConRadioAccessCodesForRadio(int radioAddress)
        {
            return TConTenantFilteredRadioAccessCodes.Where(x => x._RadioAddr == radioAddress).ToListAsync();
        }

        public async Task RemoveTConRadioAccessCodeFromId(int id)
        {
            var tConRadioAccessCode = await this.TConTenantFilteredRadioAccessCodes.SingleOrDefaultAsync(x => x.Idx == id);
            if (tConRadioAccessCode == null) return;
            this._tallyDbContext.Remove(tConRadioAccessCode);
        }

        public async Task<List<TConRadioAccessCode>> RemoveAllTConRadioAccessCodes()
        {
            var allTConRadioAccessCodes = await FindAllTConRadioAccessCodes();
            foreach (var radioAccessCode in allTConRadioAccessCodes)
            {
                await this.RemoveTConRadioAccessCodeFromId(radioAccessCode.Idx);
            }
            return allTConRadioAccessCodes;
        }

        public Task<List<TConStatusTBD>> FindAllTConTBDStatuses()
        {
            return TConTenantFilteredTDBStatus.ToListAsync();
        }

        public Task<TConStatusTBD> FindSingleTConTBDStatus(int radioAddress)
        {
            return _tallyDbContext.TConTBDStatuses.SingleAsync(x => x.RadioAddr == radioAddress);
        }

        public Task<List<TConTBDAccessLog>> FindTConTBDAccessLogs(int radioAddress, int takeCount)
        {
            return _tallyDbContext.TConTBDAccessLogs.Where(x => x.RadioAddr == radioAddress).OrderByDescending(x => x.DateTime).Take(takeCount).ToListAsync();
        }

        private void CacheRadioTypes(List<TConRadio> radios)
        {
            _cachedRadioTypes = radios.ToDictionary(x => x.RadioAddr, x => x.Type);
        }

        public async Task MarkCodeAsSettled(int code)
        {
            var unsettledTBE = await TConTenantFilteredTBEConsumptions.Where(x => !x.__Recorded && x.OpenByCode == code).ToListAsync();
            unsettledTBE.ForEach(x => x.__Recorded = true);

            var unsettledTBD = await TConTenantFilteredTBDAccessLogs.Where(x => !x.__Recorded && x.Code == code).ToListAsync();
            unsettledTBD.ForEach(x => x.__Recorded = true);
        }

        public Task<List<TConTWIStatus>> FindAllTConTWIControllers(TWIModus? filterModus)
        {
            return TConTenantFilteredTWIControllers.Where(x => filterModus == null || x.Modus == (byte)filterModus.Value).ToListAsync();
        }

        public Task<TConTWIStatus> FindSingleTConTWIController(int radioAddress)
        {
            return _tallyDbContext.TConTWIStatuses.SingleAsync(x => x.RadioAddr == radioAddress);
        }
    }
}