using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Enums;

namespace NxB.Dto.DocumentApi
{
    public class CreateBatchDto
    {
        public string Name { get; set; }
        public Guid? DocumentTemplateSmsId { get; set; }
        public BatchType BatchType { get; set; }
    }

    public class CreateDueBatchDto : CreateBatchDto
    {
        public DateTime VoucherDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal DueAmount { get; set; }
        public string Note { get; set; }
    }

    public class CopyBatchDto
    {
        public Guid SourceBatchId { get; set; }
        public string DestinationName { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? VoucherDate { get; set; }
        public decimal? DueAmount { get; set; }
        public string Note { get; set; }
    }

    public class BatchDto : CreateBatchDto
    {
        public Guid Id { get; set; }
        public string CreateAuthorName { get; set; }
        public DateTime CreateDate { get; set; }
        public Guid TenantId { get; set; }
        public string BatchTypeClass { get; set; }
        public Guid? PreviewFileId { get; set; }
        public DateTime? PreviewCreateDate { get; set; }
        public Guid DocumentTemplateId { get; set; }
        public string DocumentTemplateName { get; set; }
        public string DocumentTemplateSmsName { get; set; }
        public string EmailSubjectPrefix { get; set; }
        public bool IsArchived { get; set; }
        public Guid? JobId { get; set; }
        public int JobsCount { get; set; }
        public int JobsSuccess { get; set; }
        public int JobsFailure { get; set; }

        public DateTime? DueDate { get; set; }
        public DateTime? VoucherDate { get; set; }
        public decimal? DueAmount { get; set; }
        public string Note { get; set; }

        public virtual List<BatchItemDto> BatchItems { get; set; } = new();
    }

  public class CreateBatchItemDto
    {
        public Guid BatchId { get; set; }
        public string RecipientName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public Guid CustomerId { get; set; }
        public long FriendlyCustomerId { get; set; }
        public Guid? OrderId { get; set; }
        public int? FriendlyOrderId { get; set; }
        public Guid? VoucherId { get; set; }
        public long? FriendlyVoucherId { get; set; }
        public VoucherType? VoucherType { get; set; }
        public bool SendSms { get; set; }
        public string Language { get; set; }
        public string CountryId { get; set; }
    }

    public class CreateDueBatchItemDto : CreateBatchItemDto
    {
        public Guid AccountId { get; set; }
    }

    public class BatchItemDto : CreateBatchItemDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Guid? JobTaskId { get; set; }
        public Guid? MessageId { get; set; }
        public Guid? PreviewFileId { get; set; }
        public DateTime? PreviewCreateDate { get; set; }
        public int Index { get; set; }
        public string JobTaskErrorMessage;
        public JobTaskStatus? JobTaskStatus { get; set; }
        public DateTime? jobsLastRun { get; set; }
        public DateTime? JobTaskLastRun { get; set; }
        public decimal? JobTaskDuration { get; set; }
        public DeliveryStatus? MessageDeliveryStatus { get; set; }
        public string MessageDeliveryError { get; set; }
        public DateTime? MessageDeliveryDate { get; set; }
        public bool? MessageIsReadByRecipient { get; set; }
        public DateTime MessageIsReadByRecipientDate { get; set; }
        public string MessageAttachmentsJson { get; set; }
        public DeliveryStatus? SmsMessageDeliveryStatus { get; set; }
        public string SmsMessageDeliveryError { get; set; }
        public DateTime? SmsMessageDeliveryDate { get; set; }
        public Guid? AccountId { get; set; }
        public bool? VoucherIsClosed { get; set; }
        public DateTime? VoucherCloseDate { get; set; }
        public DateTime? PaymentAuthorizeCreateDate { get; set; }
        public string PaymentAuthorizeTransactionType { get; set; }
        public decimal? PaymentAuthorizeAmount { get; set; }
        public DateTime? PaymentCaptureCreateDate { get; set; }
        public string PaymentCaptureTransactionType { get; set; }
        public decimal? PaymentCaptureAmount { get; set; }
    }

    public class BatchTotalsDto
    {
        public Guid Id { get; set; }
        public Guid? JobId { get; set; }
        public int JobsCount { get; set; }
        public int JobsSuccess { get; set; }
        public int JobsFailure { get; set; }
    }

    public enum BatchType
    {
        None,
        EmailAndSms,
        Email,
        Sms
    }
}
