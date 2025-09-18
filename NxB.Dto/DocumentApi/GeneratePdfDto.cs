using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Munk.Utils.Object;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Model;

namespace NxB.Dto.DocumentApi
{
    public class GeneratePdfDto
    {
        public string Type { get; set; } = "document";

        [NoEmpty]
        public Guid DocumentTemplateId { get; set; }
        public string OrderId { get; set; }

        public string Languages { get; set; }
        public Guid? SaveId { get; set; }

        public PrintSettingsExtended PrintSettingsExtended { get; set; }

        public string OverrideDocumentText { get; set; }
        public PdfGenerationPriority PdfGenerationPriority { get; set; }

        public DateTime? VoucherDate { get; set; }
        public DateTime? DueDate { get; set; }
        public Guid? AccountId { get; set; }
        public decimal? DueAmount { get; set; }
        public string Note { get; set; }
    }
}
