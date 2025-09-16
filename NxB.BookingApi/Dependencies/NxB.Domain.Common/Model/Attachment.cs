using System;

namespace NxB.Domain.Common.Model
{
    public class Attachment
    {
        public Guid FileId { get; set; }
        public string FileName { get; set; }
        public string Type { get; set; }
    }
}
