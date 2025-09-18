using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NxB.Domain.Common.Enums;
using NxB.Dto.LogApi;

namespace NxB.Clients.Interfaces
{
    public interface IApplicationLogClient : IAuthorizeClient
    {
        Task AppendLog(CreateApplicationLogDto applicationLogDto, Guid? tenantId = null);
        Task TryAppendLog(CreateApplicationLogDto applicationLogDto, Guid? tenantId = null);
        void AppendLogFireAndForget(CreateApplicationLogDto applicationLogDto, Guid? tenantId = null);
        Task TryAppendTrace(ApplicationLogType applicationLogType, LogVisibilityType visibilityType, string text, Guid? tenantId = null);
        Task TryAppendError(ApplicationLogType applicationLogType, LogVisibilityType visibilityType, string text, Guid? tenantId = null);
        Task<List<ApplicationLogDto>> FindLogsFromCustomParam1(string customParam1, Guid tenantId);
    }
}
