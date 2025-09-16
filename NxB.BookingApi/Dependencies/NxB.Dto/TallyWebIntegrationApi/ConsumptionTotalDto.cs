using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Dto.TallyWebIntegrationApi
{
    public class ConsumptionTotalDto
    {
        public int Idx { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime CreateDateDB { get; set; }
        public int RadioAddress { get; set; }
        public string RadioName { get; set; }
        public bool IsKeyCode { get; set; }
        public uint Code { get; set; }
        public int PulseTime { get; set; }
        public decimal Consumed { get; set; }
    }
}
