using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Model;

namespace NxB.Dto.TallyWebIntegrationApi
{
    public class RadioAccessCodeDto
    {
        public int Id { get; set; }
        public TConRadioAccessNewState New { get; set; }
        public int RadioAddress { get; set; }
        public uint Code { get; set; }
        public bool IsKeyCode { get; set; }
        public bool IsActive { get; set; }
        public int Option { get; set; }
    }

    public class BaseCreateRadioAccessDto
    {
        public uint? Code { get; set; }
        public bool IsKeyCode { get; set; }
    }

    public class CreateRadioAccessDto : TallyRadiosFilter
    {
        public uint? Code { get; set; }
        public bool IsKeyCode { get; set; }
        public int Option { get; set; } = 0;
    }

    public class CreateRadioAccessFromAccessibleItemsDto 
    {
        public bool IsKeyCode { get; set; }
        public uint? Code { get; set; }
        public AccessibleItems AccessibleItems { get; set; } = new();

        public Guid? TenantId { get; set; }
    }
}
