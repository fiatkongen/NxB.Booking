using System;
using NxB.Domain.Common.Enums;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class TConSocketTWEV : ITConSocket
    {
        public TConRadioType SocketType => TConRadioType.TWEV;
        public int RadioAddr { get; private set; }
        public byte SocketNo { get; private set; }
        public bool OnLine { get; private set; }
        public string _Name { get; set; }
        public bool On { get; private set; }
        public double _Consumption { get; set; }
        public double StartConsumption { get; set; }
        public DateTime? StartDateTime { get; private set; }
        public DateTime? EndDateTime { get; private set; }
        public byte _SetOnOffState { get; set; }
        public bool LockStatus { get; set; }
        public byte ClosedBy { get; private set; }
        public bool Discon { get; set; }
        public byte _CurrentLimitL1 { get; set; }
        public byte _CurrentLimitL2 { get; set; }
        public byte _CurrentLimitL3 { get; set; }
        public byte EVstate { get; private set; }
        public bool LockError { get; set; }
        public int Watt { get; set; }
        public double AmpereL1 { get; set; }
        public double AmpereL2 { get; set; }
        public double AmpereL3 { get; set; }
        public double VoltageL1 { get; set; }
        public double VoltageL2 { get; set; }
        public double VoltageL3 { get; set; }
        public bool ControllerError { get; set; }
        public bool MeterError { get; set; }
        public bool PhaseSequence { get; set; }
        public bool _Enabled { get; set; }
        public DateTime LastUpdate { get;  set; }
    }
}