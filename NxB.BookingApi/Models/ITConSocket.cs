using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Enums;

namespace NxB.BookingApi.Models
{
    public interface ITConSocket
    {
        int RadioAddr { get; }
        byte SocketNo { get; }
        bool OnLine { get; }
        string _Name { get; set; }
        bool On { get; }
        double _Consumption { get; set; }
        double StartConsumption { get; set; }
        DateTime? StartDateTime { get; }
        DateTime? EndDateTime { get; }
        byte _SetOnOffState { get; set; }
        byte ClosedBy { get; }
        DateTime LastUpdate { get; }
        TConRadioType SocketType { get; }
    }

    public interface ITConSocketPulse : ITConSocket
    {
        bool _Water { get; set; }
        bool _Enabled { get; set; }
        double _PulseSetup { get; set; }
        byte _SetSetupState { get; set; }
        bool AlarmPwrOff { get; }
        bool AlarmPlugRemove { get; }
        bool AlarmWaterLeak { get; }
    }
}
