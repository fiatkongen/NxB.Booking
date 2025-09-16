using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Enums;

namespace NxB.Dto.TallyWebIntegrationApi
{
    public class SwitchDto
    {
        public int RadioAddress { get; set; }
        public string Name { get; set; }
        public bool IsLocked { get; set; }
        public bool IsOnline { get; set; }
        public TConOnOffState OnOffState { get; set; }
        public bool IsOpenedByKeyCode { get; set; }
        public int OpenByCode { get; set; }
        public int SequenceNo { get; set; }
        public int PulseTime { get; set; }
    }
}