using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Dto.TallyWebIntegrationApi
{
    public class SwitchLogDto
    {
        public int Id { get; set; }
        public int RadioAddress { get; set; }
        public bool IsOpenedByKeyCode { get; set; }
        public uint OpenByCode { get; set; }
        public int SequenceNo { get; set; }
        public bool IsRejected { get; set; }
        public bool IsSettled { get; set; }
        public int PulseTime { get; set; }
        public DateTime RadioCreateDateTime { get; set; }
        public DateTime CreateDateTime { get; set; }
    }
}
