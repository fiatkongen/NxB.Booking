using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Munk.Utils.Object;
using Newtonsoft.Json;
using NxB.Domain.Common.Constants;
using NxB.Domain.Common.Model;
using NxB.Clients.Interfaces;
using NxB.Dto.DocumentApi;
using NxB.Dto.JobApi;
using ServiceStack;

namespace NxB.Clients
{
    public class JobClient : NxBClient, IJobClient
    {
        public static string SERVICEURL = "/NxB.Services.App/NxB.JobApi";

        public JobClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task<JobDto> QueueJob(List<ICreateJobTask> createJobTask, JobPriority priority = JobPriority.Medium, bool appendCurrentRequestPath = true, bool skipBroadcast = false, Guid? appendToJobId = null)
        {
            var createJobDto = CreateOrAppendToJobDto(createJobTask, priority, appendCurrentRequestPath);
            var url = $"{SERVICEURL}/job/queue" + (skipBroadcast ? "?skipBroadcast=true" : "?skipBroadcast=false")+
                      (appendToJobId != null ? $"&appendToJobId={appendToJobId.Value}" : "");
            var jobDto = await this.PostAsync<JobDto>(url, createJobDto);
            return jobDto;
        }

        public async Task<JobDto> StopJob(Guid id)
        {
            var url = $"{SERVICEURL}/job/stop?id={id}";
            return await this.PutAsync<JobDto>(url, null);
        }

        public async Task StopJobTask(Guid id)
        {
            var url = $"{SERVICEURL}/job/jobtask/stop?id={id}";
            await this.PutAsync(url, null);
        }

        public async Task QueueJobTask(Guid id)
        {
            var url = $"{SERVICEURL}/job/jobtask/queue?id={id}";
            await this.PutAsync(url, null);
        }

        public async Task DeleteJob(Guid id)
        {
            var url = $"{SERVICEURL}/job?id={id}";
            await this.DeleteAsync(url);
        }

        public async Task<JobDto> FindSingle(Guid id)
        {
            var url = $"{SERVICEURL}/job";
            return await this.GetAsync<JobDto>(url);
        }

        public async Task<JobDto> FindSingleOrDefault(Guid id)
        {
            var url = $"{SERVICEURL}/job";
            return await this.GetAsync<JobDto>(url);
        }

        public async Task<List<JobDto>> FindAll(bool includeArchived)
        {
            var url = $"{SERVICEURL}/job/list/all";
            return await this.GetAsync<List<JobDto>>(url);
        }

        public async Task QueueJobTasks(List<Guid> jobTaskIdsToQueue)
        {
            var url = $"{SERVICEURL}/job/jobtask/list/queue";
            await this.PutAsync(url, jobTaskIdsToQueue);
        }

        public async Task DeleteJobTask(Guid? id)
        {
            var url = $"{SERVICEURL}/job/jobtask?id={id}";
            await this.DeleteAsync(url);
        }

        private CreateJobDto CreateOrAppendToJobDto(List<ICreateJobTask> createJobTask, JobPriority jobPriority, bool appendCurrentRequestPath = true)
        {
            Debug.Assert(_httpContextAccessor.HttpContext != null, "_httpContextAccessor.HttpContext != null");
            var request = _httpContextAccessor.HttpContext.Request;

            var createJobDto = new CreateJobDto
            {
                Priority = jobPriority,
                JobTasks = createJobTask.Select(x =>
                {

                    var payloadUrl = x.ServiceUrl + (appendCurrentRequestPath ? request.Path : "");
                    var payloadJson = JsonConvert.SerializeObject(x.PayloadDto);

                    var jobTask = new CreateJobTaskDto
                    {
                        Name = x.Name,
                        FriendlyOrderId = x.FriendlyOrderId,
                        OrderId = x.OrderId,
                        CustomerId = x.CustomerId,
                        FriendlyCustomerId = x.FriendlyCustomerId,
                        VoucherId = x.VoucherId,
                        FriendlyVoucherId = x.FriendlyVoucherId,
                        MessageId = x.MessageId,
                        BatchItemId = x.BatchItemId,
                        ServiceUrl = payloadUrl,
                        PayloadJson = payloadJson,
                        Debug = x.Debug,
                    };
                    return jobTask;
                }).ToList()
            };
            return createJobDto;
        }

        public class CreateJobTask : ICreateJobTask
        {
            public string ServiceUrl { get; set; }
            public string ControllerUrl { get; set; }
            public object PayloadDto { get; set; }
            public string Name { get; set; }
            public bool DependUponPreviousTask { get; set; } = false;
            public bool Debug { get; set; }
            public Guid? OrderId { get; set; }
            public int? FriendlyOrderId { get; set; }
            public Guid? CustomerId { get; set; }
            public int? FriendlyCustomerId { get; set; }
            public Guid? VoucherId { get; set; }
            public int? FriendlyVoucherId { get; set; }
            public Guid? MessageId { get; set; }
            public Guid? BatchItemId { get; set; }

            public CreateJobTask(string serviceUrl, object payloadDto, string name, bool debug, Guid? orderId = null, int? friendlyOrderId = null,
                Guid? customerId = null, int? friendlyCustomerId = null, Guid? voucherId = null, int? friendlyVoucherId = null, Guid? messageId = null, string controllerUrl = null, bool dependUponPreviousTask = false, Guid? batchItemId = null)
            {
                ServiceUrl = serviceUrl;
                PayloadDto = payloadDto;
                Name = name;
                Debug = debug;
                BatchItemId = batchItemId;
                OrderId = orderId;
                FriendlyOrderId = friendlyOrderId;
                CustomerId = customerId;
                FriendlyCustomerId = friendlyCustomerId;
                VoucherId = voucherId;
                FriendlyVoucherId = friendlyVoucherId;
                MessageId = messageId;
                ControllerUrl = controllerUrl;
                DependUponPreviousTask = dependUponPreviousTask;
            }
        }
    }
}
