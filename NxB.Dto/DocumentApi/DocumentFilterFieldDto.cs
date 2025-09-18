using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Enums;

namespace NxB.Dto.DocumentApi
{
    public class DocumentFilterFieldDto
    {
        public Guid Id { get; set; }
        public long LegacyId { get; set; }
        public string FilterClassName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public string ParametersExplanation { get; set; }
        public DocumentFilterFieldType DocumentFilterFieldType { get; set; }
    }

    public class ModifyDocumentFilterFieldDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParametersExplanation { get; set; }
    }
}
