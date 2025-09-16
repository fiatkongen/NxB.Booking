using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NxB.Dto.DocumentApi;

namespace NxB.Dto.Clients
{
    public interface IBatchClient : IAuthorizeClient
    {
        Task<BatchDto> ModifyBatch(Guid batchId, IDictionary<string, object> properties);
        Task<BatchDto> ResetBatch(Guid batchId, IDictionary<string, object> properties = null);
        Task<BatchDto> ModifyBatchItem(Guid batchItemId, IDictionary<string, object> properties);
        Task<BatchDto> ModifyJobIds(Guid batchId, Guid batchJobId, IDictionary<Guid, Guid> batchItemJobTaskIds);
    }
}
