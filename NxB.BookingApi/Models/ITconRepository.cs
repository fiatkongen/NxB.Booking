using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    public interface ITConRepository : ICloneWithCustomClaimsProvider<ITConRepository>
    {
        Task<List<TConMasterRadio>> FindAllTConMasterRadios();
        Task<List<TConRadio>> FindAllRadios();
        Task<TConRadio> FindSingleRadio(int radioAddress);
        Task<List<TConRadio>> FindAllTConTBBRadios();
        Task<List<TConRadio>> FindAllTConTBDRadios();
        Task<List<TConRadio>> FindAllTConTBERadios();
        Task<List<TConRadio>> FindAllTConTWIRadios();
        Task<List<TConRadio>> FindAllTConTWEVRadios();
        Task<TConRadio> FindSingleTConTBBRadio(int radioAddress);
        Task<TConRadio> FindSingleTConTBDRadio(int radioAddress);
        Task<TConRadio> FindSingleTConTBDOrTWIRadio(int radioAddress);
        Task<TConRadio> FindSingleTConTWCRadio(int radioAddress);
        Task<TConRadio> FindSingleTConTWEVRadio(int radioAddress);
        Task<TConRadio> FindSingleTConTWIRadio(int radioAddress);
        Task<TConRadioType> GetRadioType(int radioAddress);

        Task<List<ITConSocket>> FindAllTConSockets();
        Task<List<TConSocketTBB>> FindAllTConTBBSockets();
        Task<List<TConSocketTWC>> FindAllTConTWCSockets();
        Task<List<TConSocketTWEV>> FindAllTConTWEVSockets();
        Task<TConSocketTBB> FindSingleTConTBBSocket(int radioAddress, int socketNo);
        Task<TConSocketTWC> FindSingleTConTWCSocket(int radioAddress, int socketNo);
        Task<TConSocketTWEV> FindSingleTConTWEVSocket(int radioAddress, int socketNo);
        Task<TConSocketTBB> FindSingleOrDefaultTConTBBSocket(int radioAddress, int socketNo);
        Task<TConSocketTWC> FindSingleOrDefaultTConTWCSocket(int radioAddress, int socketNo);

        Task<List<TConTWCConsumption>> FindTConTBESocketConsumptions(int radioAddress, int socketNo);
        Task<List<TConTBBConsumption>> FindTConTBBSocketConsumptions(int radioAddress, int socketNo);
        Task<List<TConTWEVConsumption>> FindTConTWECSocketConsumptions(int radioAddress, int socketNo);

        void AddTConRadioAccessCode(TConRadioAccessCode tconRadioAccessCode);
        Task<int> RemoveTConRadioAccessWithCode(int code);
        Task RemoveTConRadioAccessCodeFromSingleRadio(int radioAddress, int code);
        Task<TConRadioAccessCode> FindTConRadioAccessCode(int radioAddress, int code);
        Task<List<TConRadioAccessCode>> FindAllTConRadioAccessCodes();
        Task<List<TConRadioAccessCode>> FindTConRadioAccessCodesWithCode(int code);
        Task<List<TConRadioAccessCode>> FindTConRadioAccessCodesForRadio(int radioAddress);
        Task RemoveTConRadioAccessCodeFromId(int id);
        Task<List<TConRadioAccessCode>> RemoveAllTConRadioAccessCodes();

        Task<List<TConStatusTBD>> FindAllTConTBDStatuses();
        Task<TConStatusTBD> FindSingleTConTBDStatus(int radioAddress);
        Task<List<TConTBDAccessLog>> FindTConTBDAccessLogs(int radioAddress, int takeCount);
        Task MarkCodeAsSettled(int code);

        Task<List<TConTWIStatus>> FindAllTConTWIControllers(TWIModus? filterModus);
        Task<TConTWIStatus> FindSingleTConTWIController(int radioAddress);
    }
}
