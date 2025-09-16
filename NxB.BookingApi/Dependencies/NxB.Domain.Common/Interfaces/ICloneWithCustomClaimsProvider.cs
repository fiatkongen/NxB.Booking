using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Domain.Common.Interfaces
{
    public interface ICloneWithCustomClaimsProvider<out TType>
    {
        TType CloneWithCustomClaimsProvider(IClaimsProvider overrideClaimsProvider);
    }
}
