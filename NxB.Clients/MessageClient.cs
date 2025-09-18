using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Domain.Common.Interfaces;
using NxB.Clients.Interfaces;
using NxB.Dto.DocumentApi;

namespace NxB.Clients
{
    public class MessageClient : NxBAdministratorClient, IMessageClient
    {
        public MessageClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task<MessageDto> FindMessage(Guid id)
        {
            var url = $"/NxB.Services.App/NxB.DocumentApi/message?id={id}";
            return await this.GetAsync<MessageDto>(url);
        }

        public async Task<MessageDto> CreateIntegrationMessage(CreateIntegrationMessageDto dto)
        {
            var url = $"/NxB.Services.App/NxB.DocumentApi/message/integration";
            var result = await this.PostAsync<MessageDto>(url, dto);
            return result;
        }

        public async Task<MessageDto> FindMessageFromFileId(Guid fileId)
        {
            var url = $"/NxB.Services.App/NxB.DocumentApi/message/fileid?fileId={fileId}";
            return await this.GetAsync<MessageDto>(url);
        }
    
        public async Task<MessageDto> FindMostRecentEmailMessageForOrderId(string orderId, MessageType? messageType)
        {
            var url = $"/NxB.Services.App/NxB.DocumentApi/message/order/recent?orderId={orderId}&messageType={(int?)messageType}";
            return await this.GetAsync<MessageDto>(url);
        }
    }
}
