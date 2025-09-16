using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Model;

namespace NxB.Dto.TallyWebIntegrationApi
{
    public class CreateAccessGroupDto : TallyRadiosFilter
    {
        public string Name { get; set; }
        public int Sort { get; set; }
    }

    public class ModifyAccessGroupDto : CreateAccessGroupDto
    {
        public Guid Id { get; set; }
    }

    public class AccessGroupDto : CreateAccessGroupDto
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
    }
}
