using System;
using System.Collections.Generic;
using System.Text;
using Munk.Utils.Object;
using NxB.Domain.Common.Enums;

namespace NxB.Domain.Common.Model
{
    [Serializable]
    public class FamilyMember : ValueObject<FamilyMember>
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public FamilyMemberType Type { get; set; }
        public string Name { get; set; }
        public int Index { get; set; }
        public bool IsDeleted { get; set; }
    }
}
