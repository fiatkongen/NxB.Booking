using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.ApplicationInsights;
using NxB.Domain.Common.Constants;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Model;
using NxB.Dto.TenantApi;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class BillingService : IBillingService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IBillableItemsRepository _billableItemsRepository;
        private readonly BillableItemFactory _billableItemFactory;
        private readonly TelemetryClient _telemetryClient;

        public BillingService(AppDbContext appDbContext, IMapper mapper, IBillableItemsRepository billableItemsRepository, BillableItemFactory billableItemFactory, TelemetryClient telemetryClient)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _billableItemsRepository = billableItemsRepository;
            _billableItemFactory = billableItemFactory;
            _telemetryClient = telemetryClient;
        }

        public async Task<BillableItemDto> CreateBillableItem(CreateBillableItemDto createBillableItemDto, Guid tenantId)
        {
            var temporaryClaimsProvider = new TemporaryClaimsProvider(tenantId, AppConstants.ADMINISTRATOR_ID, "Administrator", null, null);
            var billableItemsRepository = _billableItemsRepository.CloneWithCustomClaimsProvider(temporaryClaimsProvider);

            if (createBillableItemDto.AvoidDuplicateFromText && createBillableItemDto.Text != null)
            {
                var existingItems = await billableItemsRepository.FindFromText(createBillableItemDto.Type, createBillableItemDto.Text);
                if (existingItems.Count > 0)
                {
                    return null;
                }
            }

            if (!string.IsNullOrEmpty(createBillableItemDto.Text) && createBillableItemDto.Text.Length > 200)
            {
                createBillableItemDto.Text = createBillableItemDto.Text.Substring(0, 200);
            }
            var billableItem = new BillableItemFactory(temporaryClaimsProvider).Create(createBillableItemDto.Number, createBillableItemDto.Price, createBillableItemDto.Type, createBillableItemDto.BilledItemRef);
            _mapper.Map(createBillableItemDto, billableItem);

            await billableItemsRepository.Add(billableItem);
            await _appDbContext.SaveChangesAsync();

            if (createBillableItemDto.Type == BillableItemType.PaymentLink)
            {
                LogPaymentLinkPaid();
            }

            var billableItemDto = _mapper.Map<BillableItemDto>(billableItem);
            return billableItemDto;
        }

        private void LogPaymentLinkPaid()
        {
            var metrics = new Dictionary<string, double>
            {
                { "PaymentLinkPaid", 1 },
            };

            _telemetryClient.TrackEvent("PaymentLinkMetrics", metrics: metrics);
        }

        public async Task<BillableItemDto> TryCreateBillableItem(CreateBillableItemDto createBillableItemDto, Guid tenantId)
        {
            try
            {
                return await this.CreateBillableItem(createBillableItemDto, tenantId);
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackException(exception);
                return null;
            }
        }
    }
}