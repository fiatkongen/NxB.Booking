using System;

namespace NxB.Domain.Common.Interfaces
{
    public interface IOrderKey
    {
        Guid Id { get; }
        long FriendlyId { get; }
    }
}