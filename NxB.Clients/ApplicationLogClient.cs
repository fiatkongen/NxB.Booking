using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Model;
using NxB.Dto.ApplicationLogApi;
using NxB.Clients.Interfaces;
using NxB.Dto.LogApi;
using Munk.AspNetCore;

namespace NxB.Clients
{
    public class ApplicationLogClient : NxBAdministratorClientWithTenantUrlLookup, IApplicationLogClient
    {
        private readonly TelemetryClient _telemetryClient;
        public static string SERVICEURL = "/NxB.Services.App/NxB.LogApi";

        public ApplicationLogClient(IHttpContextAccessor httpContextAccessor, TelemetryClient telemetryClient) : base(httpContextAccessor)
        {
            _telemetryClient = telemetryClient;
        }

        public async Task AppendLog(CreateApplicationLogDto createApplicationLogDto, Guid? tenantId = null)
        {
            var url = $"{SERVICEURL}/applicationlog{(tenantId != null ? $"?tenantId={tenantId.Value}" : "")}";
            createApplicationLogDto.Text = (createApplicationLogDto.Text ?? "").Length > 500 ? createApplicationLogDto.Text.Substring(0, 500) : createApplicationLogDto.Text;
            await this.PostAsync(url, createApplicationLogDto);
        }

        public async Task TryAppendLog(CreateApplicationLogDto createApplicationLogDto, Guid? tenantId = null)
        {
            try
            {
                await this.AppendLog(createApplicationLogDto, tenantId);
            }
            catch (Exception exception) 
            {
                _telemetryClient.TrackException(exception);    
            }
        }

        public void AppendLogFireAndForget(CreateApplicationLogDto applicationLogDto, Guid? tenantId = null)
        {
            this.TryAppendLog(applicationLogDto, tenantId).FireAndForgetLogToTelemetry(_telemetryClient);
        }

        public async Task TryAppendTrace(ApplicationLogType applicationLogType, LogVisibilityType visibilityType,
            string text, Guid? tenantId = null)
        {
            try
            {
                await this.AppendLog(new CreateApplicationLogDto(applicationLogType, SeverityType.Trace, visibilityType, text), tenantId);
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackException(exception);
            }
        }

        public async Task TryAppendError(ApplicationLogType applicationLogType, LogVisibilityType visibilityType, string text, Guid? tenantId = null)
        {
            try
            {
                await this.AppendLog(new CreateApplicationLogDto(applicationLogType, SeverityType.Error, visibilityType, text), tenantId);
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackException(exception);
            }
        }

        public async Task<List<ApplicationLogDto>> FindLogsFromCustomParam1(string customParam1, Guid tenantId)
        {
            var url = $"{SERVICEURL}/applicationlog/find/customparam1?tenantId={tenantId}&customParam1={customParam1}";
            return await this.GetAsync<List<ApplicationLogDto>>(url);
        }
    }
}
