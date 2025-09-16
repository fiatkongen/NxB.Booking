using System;
using System.ComponentModel.DataAnnotations;
using Munk.Utils.Object;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Model;

namespace NxB.Dto.PdfGeneratorApi
{
    public class GeneratePdfRawDto
    {
        [Required(AllowEmptyStrings = false)]
        public string Html { get; set; }

        public PdfGenerationPriority PdfGenerationPriority { get; set; }

        [NoEmpty] 
        public PrintSettingsExtended PrintSettingsExtended { get; set; }
    }
}
