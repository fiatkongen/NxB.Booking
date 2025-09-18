using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class TConTWCConsumption
    {
        public int Idx { get; private set; }
        public int RadioAddr { get; private set; }
        public byte SocketNo { get; private set; }
        public byte ClosedBy { get; private set; }
        public bool OpenByKeyCode { get; private set; }
        public int OpenByCode { get; private set; }
        public byte SequenceNo { get; private set; }
        public bool Water { get; private set; }
        public double ConsumptionStart { get; private set; }
        public double ConsumptionEnd { get; private set; }
        public DateTime? StartDateTime { get; private set; }
        public DateTime? EndDateTime { get; private set; }
        public DateTime CreateDateTime { get; private set; }
        public bool __Recorded { get; set; }
    }
}
