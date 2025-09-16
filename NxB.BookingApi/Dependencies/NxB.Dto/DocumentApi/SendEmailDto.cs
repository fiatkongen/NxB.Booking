using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Munk.Utils.Object;

namespace NxB.Dto.DocumentApi
{
    public class SendEmailDto
    {
        [Required(AllowEmptyStrings = false)]
        public string Content { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string FromName { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string To { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Subject { get; set; }

        public string Cc { get; set; }
        public string Bcc { get; set; }

        public Guid? OrderId { get; set; }
        public long? FriendlyOrderId { get; set; }

        [NoEmpty]
        public Guid CustomerId { get; set; }
        public long FriendlyCustomerId { get; set; }

        public string CustomerSearch { get; set; }

        public List<AttachmentDto> Attachments { get; set; }

        public string SmsPdfLinkRecipients { get; set; }
    }
}
