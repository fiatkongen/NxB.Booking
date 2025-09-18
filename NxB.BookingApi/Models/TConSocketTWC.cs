using System;
using NxB.Domain.Common.Enums;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class TConSocketTWC : ITConSocketPulse
    {
        public int RadioAddr { get; private set; }
        public byte SocketNo { get; private set; }
        public bool OnLine { get; private set; }
        public string _Name { get; set; }
        public bool On { get; private set; }
        public byte ClosedBy { get; private set; }
        public bool OpenByKeyCode { get; private set; }
        public int OpenByCode { get; private set; }
        public bool Water { get; private set; }
        public double _Consumption { get; set; }
        public double StartConsumption { get; set; }
        public byte SequenceNo { get; private set; }
        public DateTime? StartDateTime { get; private set; }
        public DateTime? EndDateTime { get; private set; }
        public byte _SetOnOffState { get; set; }
        public bool AlarmPwrOff { get; private set; }
        public bool AlarmPlugRemove { get; private set; }
        public DateTime LastUpdate { get; private set; }
        public bool _Enabled { get; set; }
        public bool _Water { get; set; }
        public double _PulseSetup { get; set; }
        public bool _AlarmPlugRemoveEnabled { get; set; }
        public bool _AlarmPowerOffEnabled { get; set; }
        public byte _SetSetupState { get; set; }
        public DateTime? SetupDateTime { get; set; }
        public bool AlarmWaterLeak { get; private set; }
        public TConRadioType SocketType => TConRadioType.TBE;
    }
}