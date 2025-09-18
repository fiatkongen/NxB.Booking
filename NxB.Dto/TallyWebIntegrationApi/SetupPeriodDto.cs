using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Dto.TallyWebIntegrationApi
{
    public class SetupPeriodDto
    {
        public int MasterRadioAddress { get; set; }
        public int No { get; set; }
        public int StartHour { get; set; }
        public int StartMinute { get; set; }
        public int EndHour { get; set; }
        public int EndMinute { get; set; }
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        public bool Sunday { get; set; }
    }
}
