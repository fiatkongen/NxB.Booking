using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Enums;

namespace NxB.Dto.TenantApi
{
    public class CreateTextSectionDto
    {
        public string Text { get; set; }
        public string Summary { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public string Keywords { get; set; }
        public TextSectionType Type { get; set; }
        public string VideoUrl { get; set; }
        public int? Sort { get; set; }
        public string HelpUrlMatch { get; set; }
    }

    public class TextSectionDto : CreateTextSectionDto
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsRead { get; set; }
        public DateTime? PublishDate { get; set; }
    }

    public class ModifyTextSectionDto : CreateTextSectionDto
    {
        public Guid Id { get; set; }
    }
}
