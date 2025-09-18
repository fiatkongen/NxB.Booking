using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NxB.Dto.JobApi;

namespace NxB.Clients.Interfaces
{
    public interface ICreateJobTask
    {
        string Name { get; set; }
        string ServiceUrl { get; set; }
        object PayloadDto { get; set; }
        bool DependUponPreviousTask { get; set; }
        bool Debug { get; set; }
        Guid? OrderId { get; set; }
        int? FriendlyOrderId { get; set; } 
        Guid? CustomerId { get; set; }
        int? FriendlyCustomerId { get; set; }
        Guid? VoucherId { get; set; }
        int? FriendlyVoucherId { get; set; }
        Guid? MessageId { get; set; }
        Guid? BatchItemId { get; set; }
    }

    public interface IJobClient
    {
        Task<JobDto> QueueJob(List<ICreateJobTask> createJobTaskDto, JobPriority priority = JobPriority.Medium, bool appendCurrentRequestPath = true, bool skipBroadcast = false, Guid? appendToJobId = null);
        Task<JobDto> StopJob(Guid id);
        Task StopJobTask(Guid id);
        Task QueueJobTask(Guid id);
        Task DeleteJob(Guid id);
        Task<JobDto> FindSingle(Guid id);
        Task<JobDto> FindSingleOrDefault(Guid id);
        Task<List<JobDto>> FindAll(bool includeArchived);
        Task QueueJobTasks(List<Guid> jobTaskIdsToQueue);
        Task DeleteJobTask(Guid? id);
    }
}