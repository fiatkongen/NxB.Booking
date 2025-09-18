using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class TConTBDSetupAccess
    {
        public TConTBDSetupAccess() { }
        public TConTBDSetupAccess(byte no, int masterAddress)
        {
            this._No = no;
            this._MasterAddr = masterAddress;
        }
        public int _MasterAddr { get; private set; }
        public byte _No { get; private set; }
        public int _PulseTime { get; set; }
        public bool _Period1 { get; set; }
        public bool _Period2 { get; set; }
        public bool _Period3 { get; set; }
        public bool _Period4 { get; set; }
        public bool _Period5 { get; set; }
        public bool _Period6 { get; set; }
        public bool _Period7 { get; set; }
        public bool _Period8 { get; set; }
    }
}
