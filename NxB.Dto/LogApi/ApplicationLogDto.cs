using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Enums;

namespace NxB.Dto.LogApi
{
    public class CreateApplicationLogDto
    {
        public ApplicationLogType ApplicationLogType { get; set; }
        public SeverityType SeverityType { get; set; }
        public LogVisibilityType LogVisibilityType { get; set; }
        public bool NeedsConfirmation { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsArchived { get; set; }
        public string Text { get; set; }
        public Guid? OrderId { get; set; }
        public long? FriendlyOrderId { get; set; }
        public Guid? CustomerId { get; set; }
        public long? FriendlyCustomerId { get; set; }
        public Guid? MessageId { get; set; }
        public string CustomParam1 { get; set; }
        public string CustomParamMax { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Amount2 { get; set; }

        public CreateApplicationLogDto(ApplicationLogType applicationLogType, SeverityType severityType, LogVisibilityType logVisibilityType, string text)
        {
            ApplicationLogType = applicationLogType;
            SeverityType = severityType;
            LogVisibilityType = logVisibilityType;
            Text = text;
        }
    }

    public class ApplicationLogDto : CreateApplicationLogDto
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public Guid CreateAuthorId { get; set; }
        public Guid TenantId { get; set; }
        public string TenantName { get; set; }

        public ApplicationLogDto(ApplicationLogType applicationLogType, SeverityType severityType, LogVisibilityType logVisibilityType, string text) : base(applicationLogType, severityType, logVisibilityType, text)
        {
        }
    }
}
