using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NxB.Domain.Common.Enums;
using NxB.Dto.AccountingApi;
using NxB.Dto.DocumentApi;

namespace NxB.Clients.Interfaces
{
    public interface IDocumentClient : IAuthorizeClient
    {
        Task GenerateVoucherPdf(ReadVoucherDto invoiceDto, PaymentDto defaultPaymentDto, VoucherTemplateType voucherTemplateType, string language, Guid? documentTemplateId, string overrideDocumentText);
        Task GeneratePdfDocument(Guid documentTemplateId, string orderId, string languages, Guid? saveId = null);
        Task<DocumentTemplateDto> FindSingleDocumentTemplate(Guid documentTemplateId);
        Task<MessageDto> SendHtmlEmail(SendEmailDto sendEmailDto);
        Task<MessageDto> SendSms(SendSmsDto sendSmsDto, bool queue = true);
        Task<MessageDto> SendSmsForParentMessage(SendSmsDto sendSmsDto);
        Task<string> ShortenUrl(string url, DateTime? expiration = null);
        Task SendSmsForKioskPayment(SendSmsKioskPayment sendSmsTo);
        Task<List<OnlineDocumentTemplateDto>> FindAllGuestInfoDocumentTemplates(Guid tenantId, Guid orderId, string language, bool forMapView);
        Task<List<OnlineDocumentTemplateDto>> FindAllGuestInfoRawDocumentTemplates(Guid tenantId, string language);
        Task<List<OnlineDocumentTemplateDto>> FindAllGuestInfoEventsDocumentTemplates(Guid tenantId, string language);
    }
}
