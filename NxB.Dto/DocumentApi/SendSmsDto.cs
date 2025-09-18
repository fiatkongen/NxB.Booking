using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Munk.Utils.Object;

namespace NxB.Dto.DocumentApi
{
    public class SendSmsDto
    {
        [Required(AllowEmptyStrings = false)]
        public string To { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Content { get; set; }

        public Guid? OrderId { get; set; }
        public int? FriendlyOrderId { get; set; }

        public Guid? CustomerId { get; set; }
        public long? FriendlyCustomerId { get; set; }

        public string CustomerSearch { get; set; }

        public Guid? ParentMessageId { get; set; }
        public Guid? AlternativeSmsMessageId { get; set; }
    }
}

