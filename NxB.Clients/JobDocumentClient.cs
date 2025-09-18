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
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;
using NxB.Clients.Interfaces;
using NxB.Dto.DocumentApi;
using NxB.Dto.JobApi;
using ServiceStack;

namespace NxB.Clients
{
    public class JobDocumentClient : NxBAdministratorClientWithTenantUrlLookup, IJobDocumentClient
    {
        public static string SERVICEURL = "/NxB.Services.App/NxB.DocumentApi";

        public JobDocumentClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task CreateDocument(CreateAndSendDocumentDto createAndSendDocumentDto, bool queue = false)
        {
            var url = $"{SERVICEURL}/job/createdocument" + (queue ? "?queue=true" : "?queue=false");
            await this.PostAsync(url, createAndSendDocumentDto);
        }

        public async Task<MessageDto> CreateAndSendDocument(CreateAndSendDocumentDto createAndSendDocumentDto, bool queue = false)
        {
            var url = $"{SERVICEURL}/job/createandsenddocument" + (queue ? "?queue=true" : "?queue=false");
            var messageDto = await this.PostAsync<MessageDto>(url, createAndSendDocumentDto);
            return messageDto;
        }

        public async Task<MessageDto> CreateAndSendVoucher(CreateAndSendVoucherDto createAndSendVoucherDto, bool queue = false)
        {
            var url = $"{SERVICEURL}/job/createandsendvoucher" + (queue ? "?queue=true" : "?queue=false");
            var messageDto = await this.PostAsync<MessageDto>(url, createAndSendVoucherDto);
            return messageDto;
        }
    }
}
