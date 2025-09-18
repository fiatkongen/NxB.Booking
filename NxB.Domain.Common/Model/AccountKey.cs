using System;
using NxB.Domain.Common.Interfaces;

namespace NxB.Domain.Common.Model
{
    public class AccountKey : IAccountKey
    {
        public Guid Id { get; set; }
        public string FriendlyId { get; set; }
    }
}