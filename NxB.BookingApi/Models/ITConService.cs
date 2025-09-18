using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    // TODO: Implement TConService interface - placeholder to fix compilation errors
    // This interface was referenced in controllers but not found in the codebase
    public interface ITConService
    {
        // TODO: Implement radio access code methods
        Task<List<RadioAccessCode>> FindAllRadioAccessCodesWithCode(int code);
        Task<List<RadioAccessCode>> AddAccessBasedOnRadiosFilter(int code, bool isKeyCode, int option, object filter, List<int> radiosAdded = null);
        Task<List<RadioAccessCode>> FindAllRadioAccessCodes();
        Task<RadioAccessCode> FindRadioAccessCodeOrDefault(int radioAddress, int code);
        Task<List<RadioAccessCode>> AddAccessToSpecificSwitches(int code, bool isKeyCode, List<RadioAccessUnit> radioUnits);
        Task<List<RadioAccessCode>> RemoveAllRadioAccessCodes();
        Task RemoveRadioAccessCodeWithId(int id);
        Task RemoveRadioAccessCodesFromAllRadiosWithCode(int code);
        Task RemoveRadioAccessCodeFromSingleRadio(int radioAddress, int code);
        Task MarkCodeAsSettled(int code);

        // TODO: Implement radio methods
        Task<List<RadioBase>> FindAllRadios();
        Task<RadioBase> FindSingleRadio(int radioAddress);

        // TODO: Implement socket switch controller methods
        Task<List<SocketSwitchController>> FindAllSocketSwitchControllers(TWIModus? filterModus);
        Task SetSocketSwitchControllerModus(int radioAddress, TWIModus modus);
        Task SetSocketSwitchControllerKeypad2Enabled(int radioAddress, bool enabled);

        // TODO: Implement cloning method for tenant-specific operations
        ITConService CloneWithCustomClaimsProvider(IClaimsProvider claimsProvider);
    }

    // TODO: Implement radio access unit class - placeholder
    public class RadioAccessUnit
    {
        public int Option { get; set; }
        public int RadioAddress { get; set; }
    }
}