using NxB.Clients.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Dto.AutomationApi;
using NxB.Dto.JobApi;
using NxB.Domain.Common.Model;
using ServiceStack;

namespace NxB.Clients
{
    public class OutletClient : NxBAdministratorClient, IOutletClient
    {
        public static string SERVICEURL = "/NxB.Services.App/NxB.AutomationApi";

        public OutletClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task<OutletDto> CreateOutlet(CreateOutletDto createOutletDto)
        {
            var url = $"{SERVICEURL}/outlet";
            return await this.PostAsync<OutletDto>(url, createOutletDto);
        }

        public async Task<OutletDto> FindOutlet(Guid id)
        {
            var url = $"{SERVICEURL}/outlet?id={id}";
            return await this.GetAsync<OutletDto>(url);
        }

        public async Task<OutletDto> FindOutletFromName(string name)
        {
            var url = $"{SERVICEURL}/outlet/name?name={name}";
            return await this.GetAsync<OutletDto>(url);
        }

        public async Task<List<OutletDto>> FindAllOutlets(bool includeDeleted = false)
        {
            var url = $"{SERVICEURL}/outlet/list/all?includeDeleted={includeDeleted}";
            return await this.GetAsync<List<OutletDto>>(url);
        }

        public async Task<List<OutletDto>> FindAllOutletsFromResourceIds(List<Guid> resourceIds)
        {
            var url = $"{SERVICEURL}/outlet/list/all?includeDeleted={resourceIds.Join(",")}";
            return await this.GetAsync<List<OutletDto>>(url);
        }

        public async Task UpdateOutletMeterReading(OutletReadingDto dto)
        {
            var url = $"{SERVICEURL}/outlet/meter/reading";
            await this.PutAsync(url, dto);
        }
    }
}
