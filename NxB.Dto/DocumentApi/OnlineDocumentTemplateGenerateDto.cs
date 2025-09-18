using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Enums;

namespace NxB.Dto.DocumentApi
{
    public class OnlineDocumentTemplateGenerateDto
    {
        public string AlternateDocumentId { get; set; }
        public Guid OrderId { get; set; }
        public string Languages { get; set; }
        public Guid TenantId { get; set; }
    }
}
