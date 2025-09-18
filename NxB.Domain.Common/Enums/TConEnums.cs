namespace NxB.Domain.Common.Enums
{
    public enum TConRadioType
    {
        None = 0,
        TBB = 1,
        TBD = 5,
        TBE = 6,
        TWC_MiniMaster = 7,
        TWEV = 9,
        TWI = 10
    }

    //public enum TConRadioTypeFilter
    //{
    //    All = 0,
    //    Selected = 2,
    //    TBD = 5,
    //    TBE = 6,
    //    TWC_MiniMaster = 7,
    //    TWEV = 9,
    //    TWI = 10
    //}

    public enum TConOnOffState
    {
        NoCommandInProgress = 0,
        SwitchOffCommandInProgress = 3,
        SwitchOnCommandInProgress = 4,
        SwitchOff = 10,
        SwitchOn = 11,
    }

    public enum TConRadioAccessNewState
    {
        AccessCodeUpdated = 0,
        AddUpdateOrRemoveAccessCode = 1,
        UpdateInProgress = 2
    }

    public enum TConSetupPulseState
    {
        NoUpdateInProgress = 0,
        RequestToUpdate = 2,
        RequestUpdateEvery6Hours = 3
    }

    public enum RadioAccessUpdate
    {
        Idle = 0,
        ForceCompleteUpdateOfAccessCodes = 1
    }

    public enum RadioAccessState
    {
        AccessCodesUpdatedAndConsistent = 0,
        UpdateOfAccessCodesInProgress = 1,
        ErrorInAccessCodes = 2
    }

    public enum EVState
    {
        Unknown = 0,
        NoVehicleConnected = 1,
        CableConnected = 2,
        VehicleConnected = 3,
        VehicleReadyForChargeOrCharging_NoVentilationRequired = 4,
        VehicleReadyForChargeOrCharging_VentilationRequired = 5,
        MainsProblemRCDTripped = 6,
        ShortCircuitOrOpenPP = 7,
        ShortCircuitOrOpenCP = 8,
        LockError = 9
    }

    public  enum TWIModus
    {
        Undefined = 0,
        Consumption = 1,
        DoorOrTimer = 2,
        GateIn = 3,
        GateOut = 4
    }
}
