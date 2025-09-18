using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class TConMasterRadio
    {
        public int MasterAddr { get; private set; }
        public string SystemName { get; private set; }
        public string IPaddress { get; private set; }
        public bool OnLine { get; private set; }
        public DateTime LastOnLine { get; private set; }
        public double ScanTime { get; private set; }
        public byte _LockState { get; set; }
        public byte Quality { get; private set; }
        public int RadioOnline { get; private set; }
        public int Type { get; private set; }
        public string Version { get; private set; }
        public int FWupdateSize { get; private set; }
        public byte _TBD1updateSetup { get; set; }
        public byte LogLevel { get; private set; }
        public int GSM_RSSI { get; private set; }
        public bool GSM_Roaming { get; private set; }
    }
}
