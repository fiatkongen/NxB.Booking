using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class TConTWEVConsumption
    {
        public int Idx { get; private set; }
        public int RadioAddr { get; private set; }
        public byte SocketNo { get; private set; }
        public double ConsumptionStart { get; private set; }
        public double ConsumptionEnd { get; private set; }
        public DateTime? StartDateTime { get; private set; }
        public DateTime? EndDateTime { get; private set; }
        public byte ClosedBy { get; private set; }
        public bool __Recorded { get; set; }
    }
}
