using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class TConTWIStatus
    {
        public int RadioAddr { get; private set; }
        public bool OnLine { get; private set; }
        public byte Modus { get; private set; }
        public bool KeyPad2enable { get; private set; }
        public bool ErrorKeyPad1 { get; private set; }
        public bool ErrorKeyPad2 { get; private set; }
        public byte _SetModus { get; set; }
        public byte _SetKeyPad2enable { get; set; }
        public DateTime LastUpdate { get; private set; }
    }
}
