using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace NxB.Dto.TallyWebIntegrationApi
{
    [DataContract]
    public class TallyGateDto
    {
        [DataMember]
        public int RadioAddress { get; set; }

        [DataMember]
        public Guid TenantId { get; set; }
    }
}
