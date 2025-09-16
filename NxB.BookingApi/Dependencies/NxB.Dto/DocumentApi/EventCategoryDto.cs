using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.DocumentApi
{
    public class BaseEventCategoryDto
    {
        public Dictionary<string, string> NameTranslations { get; set; }
        public string BackgroundColor { get; set; }

        public List<Guid> DocumentTemplateActivityIds { get; set; } = new();
    }

    public class CreateEventCategoryDto : BaseEventCategoryDto
    {
        public Guid? OverrideId { get; set; }
    }

    public class ModifyEventCategoryDto : BaseEventCategoryDto
    {
        public Guid Id { get; set; }
    }

    public class EventCategoryDto : CreateEventCategoryDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
    }
}
