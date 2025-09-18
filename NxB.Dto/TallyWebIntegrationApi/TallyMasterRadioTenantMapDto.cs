using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Dto.TallyWebIntegrationApi
{
    public class CreateTallyMasterRadioTenantMapDto
    {
        public int TallyMasterRadioId { get; set; }
    }

    public class TallyMasterRadioTenantMapDto : CreateTallyMasterRadioTenantMapDto
    {
        public Guid Id { get; set; }
    }
}

