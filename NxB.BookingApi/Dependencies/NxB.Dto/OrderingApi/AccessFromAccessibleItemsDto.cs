using System;
using Munk.Utils.Object;
using NxB.Domain.Common.Model;

namespace NxB.Dto.OrderingApi
{
    public class BaseAccessFromAccessibleItemsDto
    {
        [NoEmpty]
        public Guid SubOrderId { get; set; }

        public AccessibleItems AccessibleItems { get; set; } = new();
    }

    public class CreateOrModifyAccessFromAccessibleItemsDto : BaseAccessFromAccessibleItemsDto, IAccessDto
    {
        public bool IsKeyCode { get; set; }
        public uint? Code { get; set; }
        public DateTime? AutoActivationDate { get; set; }
        public DateTime? AutoDeactivationDate { get; set; }
        public AccessType AccessType { get; set; }
    }

    public class ModifyAccessFromAccessibleItemsDto
    {
        public Guid Id { get; set; }
        public AccessibleItems AccessibleItems { get; set; } = new();
    }

    public class AccessFromAccessibleItemsDto : BaseAccessFromAccessibleItemsDto
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeactivationDate { get; set; }
        public bool IsKeyCode { get; set; }
        public uint Code { get; set; }
        public string AccessNames { get; set; }
    }
}