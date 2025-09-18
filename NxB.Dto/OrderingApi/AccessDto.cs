using System;
using System.Collections.Generic;
using System.Linq;
using Munk.Utils.Object;
using NxB.Domain.Common.Model;

namespace NxB.Dto.OrderingApi
{
    public class CreateAccessDto : TallyRadiosFilter, IAccessDto
    {
        public bool IsKeyCode { get; set; }

        [NoEmpty]
        public Guid SubOrderId { get; set; }

        public uint? Code { get; set; }

        public DateTime? AutoActivationDate { get; set; }
        public DateTime? AutoDeactivationDate { get; set; }
        public AccessType AccessType { get; set; }
    }

    public class AccessDto
    {
        public Guid Id { get; set; }
        public bool IsKeyCode { get; set; }
        public Guid SubOrderId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ActivationDate { get; set; }
        public DateTime? DeactivationDate { get; set; }
        public DateTime? AutoActivationDate { get; set; }
        public DateTime? AutoDeactivationDate { get; set; }
        public AccessType AccessType { get; set; }
        public uint Code { get; set; }
        public string AccessNames { get; set; }
        public AccessibleItems AccessibleItems { get; set; }
    }

    public enum AccessType
    {
        Default = 0,
        OneOff = 1
    }
}

