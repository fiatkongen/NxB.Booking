using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NxB.Dto.DocumentApi;

namespace NxB.Dto.Clients
{
    public interface IMessageClient : IAuthorizeClient
    {
        Task<MessageDto> FindMessage(Guid id);
        Task<MessageDto> FindMessageFromFileId(Guid fileId);
        Task<MessageDto> FindMostRecentEmailMessageForOrderId(string orderId, MessageType? messageType);
        Task<MessageDto> CreateIntegrationMessage(CreateIntegrationMessageDto dto);
    }
}
