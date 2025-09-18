using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Dto.JobApi
{
    public class QueueDocumentsJobDto<T> where T : CreateAndSendDocumentDto
    {
        public List<T> CreateAndSendDocumentDtos { get; set; }
        public Guid? UpdateBatchId { get; set; }
        public Guid? AppendToJobId { get; set; }
        public JobPriority JobPriority { get; set; }

        public List<Guid> JobTaskIdsToQueue { get; set; } = new();
    }
}
