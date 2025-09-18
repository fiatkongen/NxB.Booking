using System;
using NxB.Domain.Common.Interfaces;

namespace NxB.Domain.Common.Model
{
    public class OrderKey : IOrderKey
    {
        public Guid Id { get; set; }
        public long FriendlyId { get; set; }
    }
}