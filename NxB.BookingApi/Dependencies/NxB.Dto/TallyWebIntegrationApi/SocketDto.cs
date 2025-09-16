using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Enums;

namespace NxB.Dto.TallyWebIntegrationApi
{
    public class SocketDto
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
        public bool AlarmPwrOff { get; set; }
        public bool AlarmPlugRemove { get; set; }
        public DateTime LastUpdate { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsWater { get; set; }
        public double PulseSetup { get; set; }
        public bool AlarmWaterLeak { get; set; }
        public TConRadioType SocketType { get; set; }

        public bool? IsOpenedByKeyCode { get; set; }
        public int? OpenByCode { get; set; }
    }
}
