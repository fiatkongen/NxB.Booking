using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Enums;

namespace NxB.Dto.DocumentApi
{
    public class DocumentTemplateGenerateDto
    {
        public Guid DocumentTemplateId { get; set; }
        public string OrderId { get; set; }
        public Guid? AccountId { get; set; }
        public Guid? CustomerId { get; set; }
        public string Languages { get; set; }
        public Guid? AccessId { get; set; }
        public Guid? MessageId { get; set; }
        public Guid? FileId { get; set; }
        public Guid? TenantId { get; set; }
        public PdfGenerationPriority PdfGenerationPriority { get; set; }
        public bool SkipLanguageCleanUp { get; set; }
        public bool GenerateRaw { get; set; } = false;
    }
}
