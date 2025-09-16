using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Domain.Common.Interfaces
{
    public interface ICompositeKey
    {
        Guid Id { get; }
        long FriendlyId { get; }
    }
}
