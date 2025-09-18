using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class TConStatusTBD
    {
        public int RadioAddr { get; private set; }
        public string _Name { get; set; }
        public bool _Locked { get; set; }
        public byte _SetOnOffState { get; set; }
        public bool KeyCode { get; private set; }
        public int Code { get; private set; }
        public byte SequenceNo { get; private set; }
        public int _PulseTime { get; set; }
        public bool OnLine { get; set; }
    }
}
