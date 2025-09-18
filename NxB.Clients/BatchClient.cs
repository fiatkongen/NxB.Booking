using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Domain.Common.Interfaces;
using NxB.Clients.Interfaces;
using NxB.Dto.DocumentApi;
using ServiceStack;

namespace NxB.Clients
{
    public class BatchClient : NxBAdministratorClient, IBatchClient
    {
        public static string SERVICEURL = "/NxB.Services.App/NxB.DocumentApi";

        public BatchClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task<BatchDto> ModifyBatch(Guid batchId, IDictionary<string, object> properties)
        {
            var url = $"{SERVICEURL}/batch?id={batchId}";
            return await this.PutAsync<BatchDto>(url, properties ?? new Dictionary<string, object>());
        }

        public async Task<BatchDto> ResetBatch(Guid batchId, IDictionary<string, object> properties = null)
        {
            var url = $"{SERVICEURL}/batch/reset?id={batchId}";
            return await this.PutAsync<BatchDto>(url, properties ?? new Dictionary<string, object>());
        }

        public async Task<BatchDto> ModifyBatchItem(Guid batchItemId, IDictionary<string, object> properties)
        {
            var url = $"{SERVICEURL}/batch/item?id={batchItemId}";
            return await this.PutAsync<BatchDto>(url, properties);
        }

        public async Task<BatchDto> ModifyJobIds(Guid batchId, Guid batchJobId, IDictionary<Guid, Guid> batchItemJobTaskIds)
        {
            var url = $"{SERVICEURL}/batch/jobids?batchId={batchId}&batchJobId={batchJobId}";
            return await this.PutAsync<BatchDto>(url, batchItemJobTaskIds);
        }
    }
}
