using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using NxB.Clients.Interfaces;
using NxB.Dto.OrderingApi;
using NxB.Settings.Shared.Infrastructure;
using ServiceStack;

namespace NxB.Clients
{
    public class CtoutvertClient : NxBAdministratorClient, ICtoutvertClient
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ISettingsRepository _settingsRepository;

        public CtoutvertClient(IHttpContextAccessor httpContextAccessor, TelemetryClient telemetryClient, ISettingsRepository settingsRepository) : base(httpContextAccessor)
        {
            _telemetryClient = telemetryClient;
            _settingsRepository = settingsRepository;
        }

        public async Task PushAll(Guid tenantId)
        {
            if (!_settingsRepository.IsCtoutvertActivated(tenantId)) return;
            var url = $"/NxB.Services.App/NxB.CtoutvertApi/availability/push/all?tenantId={tenantId}";
            await this.PostAsync(url, null);
        }

        public async Task PushPriceAvailability(Guid tenantId, List<Guid> rentalCategoryIds, bool queue)
        {
            if (!_settingsRepository.IsCtoutvertActivated(tenantId)) return;
            var url = $"/NxB.Services.App/NxB.CtoutvertApi/availability/push?queue={queue}&tenantId={tenantId}";
            await this.PostAsync(url, rentalCategoryIds);
        }

        public async Task<OrderDto> Book(string xml)
        {
            var url = $"/NxB.Services.App/NxB.CtoutvertApiCtoutvertApi/orderonline/xml";
            return await this.PostAsync<OrderDto>(url, xml);
        }
    }
}
