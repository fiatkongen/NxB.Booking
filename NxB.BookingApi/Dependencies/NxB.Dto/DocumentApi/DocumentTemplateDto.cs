using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Munk.Utils.Object;
using NxB.Domain.Common.Enums;

namespace NxB.Dto.DocumentApi
{
    public class BaseDocumentTemplateDto
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public DocumentTemplateType DocumentTemplateType { get; set; }
        public VoucherTemplateType VoucherTemplateType { get; set; }
        public PrintBasicSettingsDto PrintSettings { get; set; }
        public DepositSettingsDto DepositSettings { get; set; }
        public string VisualSettings { get; set; }
        public bool IsShared { get; set; }
        public bool IsTemplate { get; set; }
        public bool IsOneOff { get; set; }
        public bool IsHidden { get; set; }
        public bool IsHiddenInUI { get; set; }
        public string AlternateId { get; set; }
        public int Sort { get; set; }
        public ImageAttachmentsDto Attachments { get; set; } = new();
        public DateTime? StartDate { get; set; }
        public bool IsHiddenWeb { get; set; }
    }

    public class CreateDocumentTemplateDto : BaseDocumentTemplateDto
    {
        public SendSmsLinkEnum SendSmsLink { get; set; }
    }

    public class ModifyDocumentTemplateDto : BaseDocumentTemplateDto
    {
        public Guid Id { get; set; }
    }

    public class DocumentTemplateDto : ModifyDocumentTemplateDto
    {
        public Guid TenantId { get; set; }
        public bool IsDeleted { get; set; }
        public SendSmsLinkEnum SendSmsLink { get; set; }
    }

    public class OnlineDocumentTemplateDto : ModifyDocumentTemplateDto
    {
        public string Translation { get; set; }
        public Dictionary<string, string> Translations { get; set; } = new();
    }

    public class CopyDocumentTemplateDto
    {
        public Guid SourceId { get; set; }
        public string NewTemplateName { get; set; }
        public bool CopyShared { get; set; }
        public DocumentTemplateType? OverrideDocumentTemplateType = null;
        public VoucherTemplateType? OverrideVoucherTemplateType = null;
        public bool IsHidden { get; set; }
        public bool IsOneOff { get; set; }
    }
}
