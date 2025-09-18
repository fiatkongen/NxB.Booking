using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NxB.Dto.DocumentApi;

namespace NxB.Clients.Interfaces
{
    public interface IReportingClient : IAuthorizeClient
    {
        public Task<int> QueryDeparturesMissingCount(DateTime departure);
        public Task<int> QueryArrivalsMissingCount(DateTime departure);
        public Task<BatchItemDto> FindSingleBatchItem(Guid? batchItemId, Guid? messageId);
        public Task<List<BatchTotalsDto>> QueryBatchTotals(Guid? tenantId = null);
    }
}
