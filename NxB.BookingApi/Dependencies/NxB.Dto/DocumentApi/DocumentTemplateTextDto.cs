using System;
using System.Collections.Generic;
using System.Text;
using Munk.Utils.Object;

namespace NxB.Dto.DocumentApi
{
    public class ModifyOrCreateDocumentTemplateTextDto
    {
        public string Text { get; set; }
        
        [NoEmpty]
        public  Guid DocumentTemplateId { get; set; }
        
        [NoEmpty]
        public string LanguageIso { get; set; }
    }
}
