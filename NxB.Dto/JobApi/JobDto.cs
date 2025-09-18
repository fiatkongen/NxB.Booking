using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Munk.Utils.Object;
using NxB.Domain.Common.Enums;

namespace NxB.Dto.JobApi
{
    public class CreateJobDto
    {
        public JobPriority Priority { get; set; } = JobPriority.Medium;
        public List<CreateJobTaskDto> JobTasks { get; set; } = new();
    }

    public class JobDto
    {
        public Guid Id { get; set; }

        public List<JobTaskDto> JobTasks { get; set; } = new();
        public List<JobTaskDto> OrderedJobTasks => JobTasks.OrderBy(x => x.Index).ToList();
    }

    public class CreateJobTaskDto
    {
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }

        public Guid? OrderId { get; set; }
        public long? FriendlyOrderId { get; set; }

        public Guid? CustomerId { get; set; }
        public long? FriendlyCustomerId { get; set; }
        public Guid? MessageId { get; set; }

        public Guid? VoucherId { get; set; }
        public long? FriendlyVoucherId { get; set; }

        public Guid? BatchItemId { get; set; }

        [NoEmpty]
        public string PayloadJson { get; set; }

        [NoEmpty]
        public string ServiceUrl { get; set; }

        public bool Debug { get; set; }
    }

    public class JobTaskDto : CreateJobTaskDto
    {
        public Guid Id { get; set; }
        public Guid? DependentJobTaskId { get; set; }
        public int Index { get; set; }
        public Guid JobId { get; set; }

        public JobTaskStatus Status { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime? LastRun { get; set; }
        public decimal? Duration { get; set; }
        public string TenantName { get; set; }
    }
}
