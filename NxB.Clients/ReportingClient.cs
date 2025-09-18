using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Domain.Common.Interfaces;
using NxB.Clients.Interfaces;
using NxB.Dto.DocumentApi;

namespace NxB.Clients
{
    public class ReportingClient : NxBAdministratorClient, IReportingClient
    {
        public ReportingClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task<int> QueryDeparturesMissingCount(DateTime departure)
        {
            var url = $"/NxB.Services.App/NxB.ReportingApi/order/query/departure/missing/count?departure={departure.ToJsonDateString()}";
            return await this.GetAsync<int>(url);
        }

        public async Task<int> QueryArrivalsMissingCount(DateTime arrival)
        {
            var url = $"/NxB.Services.App/NxB.ReportingApi/order/query/arrival/missing/count?arrival={arrival.ToJsonDateString()}";
            return await this.GetAsync<int>(url);
        }

        public async Task<BatchItemDto> FindSingleBatchItem(Guid? batchItemId, Guid? messageId)
        {
            var url = $"/NxB.Services.App/NxB.ReportingApi/batch/item?{(batchItemId != null ? $"batchItemId={batchItemId.Value}" : $"messageId={messageId.Value}")}";
            return await this.GetAsync<BatchItemDto>(url);
        }

        public async Task<List<BatchTotalsDto>> QueryBatchTotals(Guid? tenantId = null)
        {
            var url = $"/NxB.Services.App/NxB.ReportingApi/batch/list/all/totals{(tenantId != null ? $"?tenantId={tenantId.Value}" : "")}";
            return await this.GetAsync<List<BatchTotalsDto>>(url);
        }
    }
}
