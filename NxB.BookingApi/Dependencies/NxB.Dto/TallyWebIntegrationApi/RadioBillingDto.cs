using System;
using System.Collections.Generic;
using System.Text;
using Munk.Utils.Object;

namespace NxB.Dto.TallyWebIntegrationApi
{
    public class RadioBillingDto
    {
        public int RadioAddress { get; set; }
        public bool IsDisabled { get; set; }
        
        [NoEmpty]
        public Guid ArticleId { get; set; }
    }
}
