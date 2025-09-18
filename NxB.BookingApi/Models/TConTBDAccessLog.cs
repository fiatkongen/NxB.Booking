using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class TConTBDAccessLog
    {
        public int Idx { get; set; }
        public int RadioAddr { get; set; }
        public bool KeyCode { get; set; }
        public int Code { get; set; }
        public bool Rejected { get; set; }
        public int PulseTime { get; set; }
        public byte SequenceNo { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime SavedDateTime { get; set; }
        public bool __Recorded { get; set; }
    }

    public class TConTBDAccessLogExtended : TConTBDAccessLog
    {
        public int MasterAddr { get; set; }

        public TConTBDAccessLogExtended(TConTBDAccessLog accessLog, int masterAddr)
        {
            MasterAddr = masterAddr;
            Idx = accessLog.Idx;
            RadioAddr = accessLog.RadioAddr;
            KeyCode = accessLog.KeyCode;
            Code = accessLog.Code;
            Rejected = accessLog.Rejected;
            PulseTime = accessLog.PulseTime;
            SequenceNo = accessLog.SequenceNo;
            DateTime = accessLog.DateTime;
            SavedDateTime = accessLog.SavedDateTime;
            __Recorded = accessLog.__Recorded;
        }
    }
}

