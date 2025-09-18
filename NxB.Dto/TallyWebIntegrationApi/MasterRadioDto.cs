using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Dto.TallyWebIntegrationApi
{
    public class MasterRadioDto
    {
        public int MasterAddress { get; set; }
        public string SystemName { get; set; }
        public string IpAddress { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastOnline { get; set; }
        public double ScanTime { get; set; }
        public int LockState { get; set; }
        public int Quality { get; set; }
        public int RadiosOnlineCount { get; set; }
        public int Type { get; set; }
        public string Version { get; set; }
        public int FwUpdateSize { get; set; }
        public int Tbd1UpdateSetup { get; set; }
        public int LogLevel { get; set; }
        public int Gsm_RSSI { get; set; }
        public bool IsGSM_Roaming { get; set; }
    }
}
