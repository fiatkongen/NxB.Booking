using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Enums;

namespace NxB.Dto.TallyWebIntegrationApi
{
    public class ChargerDto
    {
        public int RadioAddress { get; set; }
        public int SocketNo { get; set; }
        public bool IsOnline { get; set; }
        public string Name { get; set; }
        public bool IsOn { get; set; }
        public int ClosedBy { get; set; }
        public double Consumption { get; set; }
        public double StartConsumption { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public TConOnOffState OnOffState { get; set; }
        public DateTime LastUpdate { get; set; }
        public bool LockStatus { get; set; }
        public bool Disconnected { get; set; }
        public int CurrentLimitL1 { get; set; }
        public int CurrentLimitL2 { get; set; }
        public int CurrentLimitL3 { get; set; }
        public EVState EvState { get; set; }
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
        public TConRadioType SocketType { get; set; }
    }
}
