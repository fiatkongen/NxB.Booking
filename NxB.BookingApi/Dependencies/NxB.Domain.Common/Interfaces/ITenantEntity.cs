using System;

namespace NxB.Domain.Common.Interfaces
{
    public interface ITenantEntity
    {
        Guid TenantId { get; }
    }
}
