using System;

namespace NxB.Domain.Common.Interfaces
{
    public interface IAccountKey
    {
        Guid Id { get; }
        string FriendlyId { get; }
    }
}