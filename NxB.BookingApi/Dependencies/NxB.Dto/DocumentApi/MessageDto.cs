using System;
using System.Collections.Generic;
using System.Text;
using Munk.Utils.Object;
using NxB.Domain.Common.Enums;

namespace NxB.Dto.DocumentApi
{
    public class MessageDto
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateAuthorName { get; set; }
        public MessageType MessageType { get; set; }
        public bool IsInbound { get; set; }
        public bool IsRead { get; set; }
        public bool IsReadByRecipient { get; set; }
        public DateTime? IsReadByRecipientDate { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? ReplyToMessageId { get; set; }
        public string Subject { get; set; }
        public string Recipients { get; set; }
        public string Sender { get; set; }
        public string Content { get; set; }
        public MessageFormatType MessageFormatType { get; set; }

        public Guid? OrderId { get; set; }
        public int? FriendlyOrderId { get; set; }

        public Guid? VoucherId { get; set; }
        public long? FriendlyVoucherId { get; set; }

        [NoEmpty]
        public Guid CustomerId { get; set; }
        public long FriendlyCustomerId { get; set; }

        public DeliveryStatus DeliveryStatus { get; set; }
        public string DeliveryError { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? FutureDelivery { get; set; }

        public DeliveryStatus SmsDeliveryStatus { get; set; }
        public string SmsDeliveryError { get; set; }
        public DateTime? SmsDeliveryDate { get; set; }

        public DeliveryErrorAck DeliveryErrorAck { get; set; }

        public string CustomerSearch { get; set; }
        public List<AttachmentDto> Attachments { get; set; } = new();
    }

    public class CreateIntegrationMessageDto
    {
        public string Content { get; set; }
        public Guid OrderId { get; set; }
        public long FriendlyOrderId { get; set; }
        public Guid CustomerId { get; set; }
        public long FriendlyCustomerId { get; set; }
        public string CustomerSearch { get; set; }
        public string Sender { get; set; }
        public string Subject { get; set; }
    }

    public enum MessageType
    {
        None,
        Email,
        Sms,
        Integration
    }

    public enum MessageFormatType
    {
        None,
        Text,
        Html
    }

    public enum DeliveryErrorAck
    {
        None,
        Needed,
        Acknowledged
    }
}
