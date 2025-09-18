using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Munk.Utils.Object;

namespace NxB.Dto.DocumentApi;

public class AttachmentDto
{
    [NoEmpty]
    public Guid FileId { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string FileName { get; set; }
        
    public string Type { get; set; }

    public bool RemoveAttachment { get; set; } = false;
}

public class ImageAttachmentsDto
{
    public string Settings { get; set; }
    public List<AttachmentDto> Attachments { get; set; }
}
