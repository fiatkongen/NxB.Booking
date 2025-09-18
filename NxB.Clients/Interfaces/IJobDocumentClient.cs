using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NxB.Dto.DocumentApi;
using NxB.Dto.JobApi;

namespace NxB.Clients.Interfaces
{
    public interface IJobDocumentClient : IAuthorizeClient
    {
        Task CreateDocument(CreateAndSendDocumentDto createAndSendDocumentDto, bool queue = false);
        Task<MessageDto> CreateAndSendDocument(CreateAndSendDocumentDto createAndSendDocumentDto, bool queue = false);
        Task<MessageDto> CreateAndSendVoucher(CreateAndSendVoucherDto createAndSendVoucherDto, bool queue = false);
    }
}
