using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Dto.TallyWebIntegrationApi
{
    public class SystemLogDto
    {
        public DateTime CreateDate { get; set; }
        public int Type { get; set; }
        public string Text { get; set; }
    }
}
