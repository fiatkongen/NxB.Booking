using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using NxB.BookingApi.Models;
using NxB.Dto.AccountingApi;
using NxB.Clients.Interfaces;
using NxB.Settings.Shared.Infrastructure;

namespace NxB.BookingApi.Infrastructure
{
    public class TotalsService : NxBAdministratorClient, ITotalsService
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IMapper _mapper;
        private readonly ISettingsRepository _settingsRepository;

        public TotalsService(IVoucherRepository voucherRepository, IMapper mapper, ISettingsRepository settingsRepository, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _voucherRepository = voucherRepository;
            _mapper = mapper;
            _settingsRepository = settingsRepository;
        }

        public async Task<AccountTotalsDto> CalculateAccountTotals(Guid accountId)
        {
            var accountTotals = await this.SummarizeAccountTotals(accountId);
            var accountTotalsDto = _mapper.Map<AccountTotalsDto>(accountTotals);
            accountTotalsDto.AccountId = accountId;
            accountTotalsDto.EurConversionRate = _settingsRepository.GetEurConversionRate();
            return accountTotalsDto;
        }

        private async Task<AccountTotals> SummarizeAccountTotals(Guid accountId)
        {
            var vouchers = await this._voucherRepository.FindFromAccountId<Voucher>(accountId, null);
            var totals = SummarizeTotals(vouchers);
            return totals;
        }

        public async Task<AccountTotalsDto> CalculateOrderTotals(Guid accountId, Guid orderId)
        {
            var accountTotals = await this.SummarizeOrderTotals(orderId);
            var accountTotalsDto = _mapper.Map<AccountTotalsDto>(accountTotals);
            accountTotalsDto.AccountId = accountId;
            accountTotalsDto.OrderId = orderId;
            accountTotalsDto.EurConversionRate = this._settingsRepository.GetEurConversionRate();
            return accountTotalsDto;
        }

        private async Task<AccountTotals> SummarizeOrderTotals(Guid orderId)
        {
            var vouchers = await this._voucherRepository.FindFromOrderId<Voucher>(orderId, null);
            var totals = SummarizeTotals(vouchers);
            return totals;
        }

        private static AccountTotals SummarizeTotals(List<Voucher> vouchers)
        {
            var invoiceBases = vouchers.OfType<InvoiceBase>().ToList();
            var invoicesDue = invoiceBases.OfType<Invoice>().Where(x => x.DueDate <= DateTime.Now.Date && x.IsOpen && x.FriendlyId > 0).ToList();

            var payments = vouchers.OfType<Payment>().ToList();

            var accountTotals = new AccountTotals
            {
                Invoices = invoiceBases.Sum(x => x.Total),
                InvoicesCount = invoiceBases.Count,
                DueInvoices = invoicesDue.Sum(x => x.Total),
                DueInvoicesCount = invoicesDue.Count,
                Payments = payments.Sum(x => x.Total),
                PaymentsCount = payments.Count,
            };

            return accountTotals;
        }
    }
}
