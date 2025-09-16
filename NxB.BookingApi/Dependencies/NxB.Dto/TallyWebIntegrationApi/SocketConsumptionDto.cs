using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Dto.TallyWebIntegrationApi
{
    public class SocketConsumptionDto
    {
        public int Id { get; set; }
        public int RadioAddress { get; set; }
        public int SocketNo { get; set; }
        public int ClosedBy { get; set; }
        public bool IsOpenedByKeyCode { get; set; }
        public int OpenByCode { get; set; }
        public int SequenceNo { get; set; }
        public bool Water { get; set; }
        public double ConsumptionStart { get; set; }
        public double ConsumptionEnd { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public DateTime CreateDateTime { get; set; }
    }
}
