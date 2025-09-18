using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class TConTBDSetupPeriod
    {
        public TConTBDSetupPeriod() { }
        public TConTBDSetupPeriod(byte no, int masterAddress)
        {
            this._No = no;
            this._MasterAddr = masterAddress;
        }
        public int _MasterAddr { get; private set; }
        public byte _No { get; private set; }
        public byte _StartH { get; set; }
        public byte _StartM { get; set; }
        public byte _EndH { get; set; }
        public byte _EndM { get; set; }
        public bool _Mon { get; set; }
        public bool _Tue { get; set; }
        public bool _Wed { get; set; }
        public bool _Thu { get; set; }
        public bool _Fri { get; set; }
        public bool _Sat { get; set; }
        public bool _Sun { get; set; }
    }
}
