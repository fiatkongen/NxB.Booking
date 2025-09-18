using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.AccountingApi;
using NxB.Clients.Interfaces;
using NxB.Dto.DocumentApi;
using ServiceStack;

namespace NxB.Clients
{
    public class DocumentClient : NxBAdministratorClient, IDocumentClient
    {
        public static string SERVICEURL = "/NxB.Services.App/NxB.DocumentApi";

        public DocumentClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task GenerateVoucherPdf(ReadVoucherDto voucherDto, PaymentDto defaultPaymentDto, VoucherTemplateType voucherTemplateType, string languages, Guid? documentTemplateId, string overrideDocumentText)
        {
            var url = $"{SERVICEURL}/pdfgenerator/voucher/generate/pdf?voucherTemplateType={voucherTemplateType}&languages={languages}" + (documentTemplateId.HasValue ? "&voucherTemplateId=" + documentTemplateId.Value : "");
            await this.PostAsync(url, new CreateVoucherPdfDto { VoucherDto = voucherDto, DefaultPaymentDto = defaultPaymentDto, OverrideDocumentText = overrideDocumentText});
        }

        public async Task GeneratePdfDocument(Guid documentTemplateId, string orderId, string languages, Guid? saveId = null)
        {
            var url = $"{SERVICEURL}/pdfgenerator/documenttemplate/generate/pdf?documentTemplateId={documentTemplateId}&orderId={orderId}&languages={languages}" +
                      (saveId.HasValue ? "&saveId=" + saveId.Value : "");
            await this.GetAsync(url);
        }

        public async Task<DocumentTemplateDto> FindSingleDocumentTemplate(Guid documentTemplateId)
        {
            var url = $"{SERVICEURL}/documenttemplate?documentTemplateId={documentTemplateId}";
            return await this.GetAsync<DocumentTemplateDto>(url);
        }

        public async Task<MessageDto> SendHtmlEmail(SendEmailDto sendEmailDto)
        {
            var url = $"{SERVICEURL}/email/send/html";
            return await this.PostAsync<MessageDto>(url, sendEmailDto);
        }

        public async Task<MessageDto> SendSms(SendSmsDto sendSmsDto, bool queue = true)
        {
            var url = $"{SERVICEURL}/sms/send/sms?queue={queue}";
            return await this.PostAsync<MessageDto>(url, sendSmsDto);
        }

        public async Task<MessageDto> SendSmsForParentMessage(SendSmsDto sendSmsDto)
        {
            var url = $"{SERVICEURL}/sms/send/sms/parentmessage";
            return await this.PostAsync<MessageDto>(url, sendSmsDto);
        }

        public async Task<string> ShortenUrl(string url, DateTime? expiration = null)
        {
            var callUrl = $"{SERVICEURL}/sms/shortenurl?url={url}&expiration={expiration}";
            return await this.GetAsync<string>(callUrl);
        }

        public async Task SendSmsForKioskPayment(SendSmsKioskPayment sendSmsTo)
        {
            var url = $"{SERVICEURL}/sms/send/sms/kiosk/payment";
            await this.PostAsync(url, sendSmsTo);
        }

        public async Task<List<OnlineDocumentTemplateDto>> FindAllGuestInfoDocumentTemplates(Guid tenantId, Guid orderId, string language, bool forMapView)
        {
            var url = $"{SERVICEURL}/guestinfohtmlgenerator/list/all?tenantId={tenantId}&orderId={orderId}&language={language}&forMapView={forMapView}";
            return await this.GetAsync<List<OnlineDocumentTemplateDto>>(url);
        }

        public async Task<List<OnlineDocumentTemplateDto>> FindAllGuestInfoEventsDocumentTemplates(Guid tenantId, string language)
        {
            var url = $"{SERVICEURL}/guestinfohtmlgenerator/events/list/all?tenantId={tenantId}&language={language}";
            return await this.GetAsync<List<OnlineDocumentTemplateDto>>(url);
        }

        public async Task<List<OnlineDocumentTemplateDto>> FindAllGuestInfoRawDocumentTemplates(Guid tenantId, string language)
        {
            var url = $"{SERVICEURL}/guestinfohtmlgenerator/list/all/raw?tenantId={tenantId}&language={language}";
            return await this.GetAsync<List<OnlineDocumentTemplateDto>>(url);
        }

        public async Task<string> GenerateMapHtml(Guid tenantId, string language, Guid? orderId=null)
        {
            var url = $"{SERVICEURL}/guestinfohtmlgenerator/generate/map/html?tenantId={tenantId}&language={language}{(orderId != null ? "&orderId=" + orderId : "")}";
            return await this.GetAsync<string>(url);
        }
    }
}
