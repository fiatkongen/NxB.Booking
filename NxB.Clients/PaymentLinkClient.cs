using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Dto.AccountingApi;
using NxB.Clients.Interfaces;

namespace NxB.Clients
{
    public class PaymentLinkClient : NxBAdministratorClient, IPaymentLinkClient
    {
        public PaymentLinkClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task<PaymentLinkDto> CreatePaymentLink(CreatePaymentLinkDto createPaymentLinkDto)
        {
            var url = $"/NxB.Services.App/NxB.AccountingApi/paymentlink";
            var paymentLinkDto = await this.PostAsync<PaymentLinkDto>(url, createPaymentLinkDto);
            return paymentLinkDto;
        }

        public async Task<PaymentLinkDto> FindPaymentLink(Guid id)
        {
            var url = $"/NxB.Services.App/NxB.AccountingApi/paymentlink?id=" + id;
            var paymentLinkDto = await this.GetAsync<PaymentLinkDto>(url);
            return paymentLinkDto;
        }

        public async Task<PaymentLinkOnlineDto> CreatePaymentLinkForPaymentVoucher(CreatePaymentLinkForPaymentVoucherDto createPaymentLinkDto)
        {
            var url = $"/NxB.Services.App/NxB.AccountingApi/paymentlink/payment";
            var paymentLinkDto = await this.PostAsync<PaymentLinkOnlineDto>(url, createPaymentLinkDto);
            return paymentLinkDto;
        }
    }
}
