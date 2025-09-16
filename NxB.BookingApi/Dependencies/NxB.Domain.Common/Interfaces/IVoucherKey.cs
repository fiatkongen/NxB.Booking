using System;

namespace NxB.Domain.Common.Interfaces
{
    public interface IVoucherKey
    {
        Guid Id { get; }
        long FriendlyId { get; }
    }
}