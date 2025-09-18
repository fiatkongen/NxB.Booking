using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Domain.Common.Interfaces
{
    public interface IMasterRadioIdProvider : ICloneWithCustomClaimsProvider<IMasterRadioIdProvider>
    {
        int MasterRadioId { get; }
    }
}
