using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NxB.Dto.AccountingApi;

namespace NxB.Clients.Interfaces
{
    public interface IPaymentLinkClient
    {
        Task<PaymentLinkDto> CreatePaymentLink(CreatePaymentLinkDto createPaymentLinkDto);
        Task<PaymentLinkDto> FindPaymentLink(Guid id);
        Task<PaymentLinkOnlineDto> CreatePaymentLinkForPaymentVoucher(CreatePaymentLinkForPaymentVoucherDto createPaymentLinkDto);
    }
}
