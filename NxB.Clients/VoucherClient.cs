using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Dto.AccountingApi;
using NxB.Clients.Interfaces;
using ServiceStack;

namespace NxB.Clients
{
    public class VoucherClient : NxBAdministratorClient, IVoucherClient
    {
        private readonly Dictionary<Guid, List<PaymentDto>> _orderPaymentDtosCache = new();
        private readonly Dictionary<Guid, List<VoucherDto>> _openPaymentsAndVouchersDtosCache = new();

        public VoucherClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task<InvoiceDto> FindInvoice(Guid id)
        {
            var url = $"/NxB.Services.App/NxB.AccountingApi/voucher/invoice?invoiceId={id}";
            var invoiceDto = await this.GetAsync<InvoiceDto>(url);
            return invoiceDto;
        }

        public async Task<List<PaymentDto>> FindPaymentsFromOrder(Guid orderId)
        {
            if (_orderPaymentDtosCache.ContainsKey(orderId))
            {
                return _orderPaymentDtosCache[orderId];
            }

            var url = $"/NxB.Services.App/NxB.AccountingApi/voucher/payment/list/all/order?orderId={orderId}";
            var paymentDtos = await this.GetAsync<List<PaymentDto>>(url);
            _orderPaymentDtosCache.Add(orderId, paymentDtos);
            return paymentDtos;
        }

        public async Task<List<VoucherDto>> FindPaymentsOrCreditNotesFromOrder(Guid orderId, bool? isClosed)
        {
            List<VoucherDto> paymentDtos;

            if (_openPaymentsAndVouchersDtosCache.ContainsKey(orderId))
            {
                paymentDtos = _openPaymentsAndVouchersDtosCache[orderId];
            }
            else
            {
                var url = $"/NxB.Services.App/NxB.AccountingApi/voucher/paymentandcreditnote/list/all/order?orderId={orderId}";
                paymentDtos = await this.GetAsync<List<VoucherDto>>(url);
                _openPaymentsAndVouchersDtosCache.Add(orderId, paymentDtos);
            }

            paymentDtos = paymentDtos.Where(x => isClosed == null || x.IsClosed == isClosed.Value).ToList();
            return paymentDtos;
        }

        public async Task<List<VoucherDto>> FindVouchersFromAccountId(Guid accountId, bool? isClosed)
        {
            var url = $"/NxB.Services.App/NxB.AccountingApi/voucher/list/all/account?accountId={accountId}&isClosed={isClosed}";
            var voucherDtos = await this.GetAsync<List<VoucherDto>>(url);
            return voucherDtos;
        }

        public async Task<ReadVoucherDto> FindReadVoucherFromDocumentId(Guid documentId)
        {
            var url = $"/NxB.Services.App/NxB.AccountingApi/voucher/readvoucher/documentid?documentId={documentId}";
            var voucherDto = await this.GetAsync<ReadVoucherDto>(url);
            return voucherDto;
        }

        public async Task<VoucherDto> FindVoucher(Guid id)
        {
            var url = $"/NxB.Services.App/NxB.AccountingApi/voucher?voucherId={id}";
            var voucherDto = await this.GetAsync<ReadVoucherDto>(url);
            return voucherDto;
        }

        public async Task<Dictionary<Guid, string>> GetInvoicedOrderLineIds(Guid orderId)
        {
            var url = $"/NxB.Services.App/NxB.AccountingApi/voucher/invoice/orderlineids/invoiced?orderId={orderId}";
            var invoicedOrderLineIds = await this.GetAsync<Dictionary<Guid, string>>(url);
            return invoicedOrderLineIds;
        }

        public async Task<PaymentDto> CreatePayment(CreatePaymentDto createPaymentDto)
        {
            var url = $"/NxB.Services.App/NxB.AccountingApi/voucher/payment";
            var paymentDto = await this.PostAsync<PaymentDto>(url, createPaymentDto);
            return paymentDto;
        }

        public async Task<PaymentDto> CreateSpecificPayment(CreateSpecificPaymentDto createPaymentDto)
        {
            var url = $"/NxB.Services.App/NxB.AccountingApi/voucher/payment/specific";
            var paymentDto = await this.PostAsync<PaymentDto>(url, createPaymentDto);
            return paymentDto;
        }

        public async Task<DueDepositDto> CreateDeposit(CreateDepositDto createDepositDto)
        {
            var url = $"/NxB.Services.App/NxB.AccountingApi/voucher/duedeposit";
            var dueDepositDto = await this.PostAsync<DueDepositDto>(url, createDepositDto);
            return dueDepositDto;
        }

        public async Task<List<PaymentDto>> FindSpecificPaymentsFromInvoiceId(Guid invoiceId)
        {
            var url = $"/NxB.Services.App/NxB.AccountingApi/voucher/payment/list/specific/invoice?invoiceId={invoiceId}";
            var paymentDtos = await this.GetAsync<List<PaymentDto>>(url);
            return paymentDtos;
        }

        public async Task<CreditNoteDto> FindSpecificCreditNoteFromInvoiceId(Guid invoiceId)
        {
            var url = $"/NxB.Services.App/NxB.AccountingApi/voucher/creditnote/specific/invoice?invoiceId={invoiceId}";
            var creditNoteDto = await this.GetAsync<CreditNoteDto>(url);
            return creditNoteDto;
        }

        public async Task<List<CreditNoteDto>> FindSpecificCreditNotesFromVoucherTransactionId(Guid transactionId)
        {
            var url = $"/NxB.Services.App/NxB.AccountingApi/voucher/creditnote/list/specific/closedtransaction?transactionId={transactionId}";
            var creditNoteDto = await this.GetAsync<List<CreditNoteDto>>(url);
            return creditNoteDto;
        }

        public async Task<ReadVoucherDto> CreateReadVoucherDto(CreateReadVoucherDto createReadVoucherDto)
        {
            var url = $"/NxB.Services.App/NxB.AccountingApi/voucher/duedeposit/readvoucherdto";
            var dueDepositDto = await this.PostAsync<ReadVoucherDto>(url, createReadVoucherDto);
            return dueDepositDto;
        }

        public async Task BroadcastDueDepositsCount()
        {
            var url = $"/NxB.Services.App/NxB.AccountingApi/voucher/broadcast/dudepositscount";
            await this.PostAsync(url, null);
        }

        public async Task BroadcastDueInvoicesCount()
        {
            var url = $"/NxB.Services.App/NxB.AccountingApi/voucher/broadcast/dueinvoicescount";
            await this.PostAsync(url, null);
        }

        public async Task BroadcastDueVouchersCount()
        {
            var url = $"/NxB.Services.App/NxB.AccountingApi/voucher/broadcast/duevoucherscount";
            await this.PostAsync(url, null);
        }
    }
}
