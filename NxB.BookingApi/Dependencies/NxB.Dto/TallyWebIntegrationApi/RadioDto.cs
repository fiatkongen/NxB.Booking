using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Enums;

namespace NxB.Dto.TallyWebIntegrationApi
{
    public class RadioDto
    {
        public int RadioAddress { get; set; }
        public int MasterRadioAddress { get; set; }
        public bool IsOnline { get; set; }
        public TConRadioType RadioType { get; set; }
        public int HopCount { get; set; }
        public DateTime LastOnline { get; set; }
        public int Quality { get; set; }
        public int OfflineCount { get; set; }
        public int PowerOnCount { get; set; }
        public string Version { get; set; }
        public int RSSI { get; set; }
        public int Noise { get; set; }
        public RadioAccessUpdate AccessUpdate { get; set; }
        public RadioAccessState AccessState { get; set; }
        public int AccessUserCount { get; set; }
        public int AccessSocketCount { get; set; }
    }
}
