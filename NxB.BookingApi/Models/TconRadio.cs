using System;
using NxB.Domain.Common.Enums;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class TConRadio
    {
        public int RadioAddr { get; private set; }
        public int MasterAddr { get; private set; }
        public int LinkAddr { get; private set; }
        public byte NoHop { get; private set; }
        public TConRadioType Type { get; private set; }
        public bool OnLine { get; private set; }
        public DateTime LastOnline { get; private set; }
        public short Quality { get; private set; }
        public byte TxLevel { get; private set; }
        public int OffLineCount { get; private set; }
        public int PowerOnCount { get; private set; }
        public string Version { get; private set; }
        public byte RSSI { get; private set; }
        public byte Noise { get; private set; }
        public int FWupdateAddr { get; private set; }
        public byte _AccessUpdate { get; set; }
        public byte _AccessState { get; set; }
        public int AccessUserCnt { get; private set; }
        public int AccessUserCheck { get; set; }
        public int AccessSocketCnt { get; private set; }
        public int AccessSocketCheck { get; private set; }
        public byte _SetupUpdate { get; set; }
        public byte _TBD1updateSetup { get; set; }
    }
}
