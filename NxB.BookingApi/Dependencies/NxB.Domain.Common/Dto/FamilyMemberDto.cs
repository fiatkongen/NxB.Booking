using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Enums;

namespace NxB.Domain.Common.Dto
{
    public class CreateFamilyMemberDto
    {
        public FamilyMemberType Type { get; set; }
        public string Name { get; set; }
    }

    public class FamilyMemberDto : CreateFamilyMemberDto
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
    }
}
