using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Munk.Utils.Object;
using NxB.Dto.AccountingApi;
using NxB.Dto.Clients;
using NxB.Dto.DocumentApi;
using NxB.Dto.TenantApi;
using NxB.BookingApi.Infrastructure;
using NxB.Settings.Shared.Infrastructure;

namespace NxB.BookingApi.Models
{
    public class OrderCommunicationHelper : IOrderCommunicationHelper
    {
        private readonly ITenantClient _tenantClient;
        private readonly ICustomerClient _customerClient;
        private readonly IDocumentClient _documentClient;
        private readonly IOrderRepository _orderRepository;
        private readonly ISettingsRepository _settingsRepository;
        private readonly IMessageClient _messageClient;
        private readonly TelemetryClient _telemetry;

        public OrderCommunicationHelper(ITenantClient tenantClient, ICustomerClient customerClient, IDocumentClient documentClient, IOrderRepository orderRepository, ISettingsRepository settingsRepository, TelemetryClient telemetry, IMessageClient messageClient)
        {
            _tenantClient = tenantClient;
            _customerClient = customerClient;
            _documentClient = documentClient;
            _orderRepository = orderRepository;
            _settingsRepository = settingsRepository;
            _telemetry = telemetry;
            _messageClient = messageClient;
        }

        public async Task<Guid> SendEmailForOrder(string orderId, Guid documentTemplateId, string languages, string forcedCustomerPhone = null)
        {
            var documentTemplate = await _documentClient.FindSingleDocumentTemplate(documentTemplateId);

            Guid fileId = Guid.NewGuid();
            await _documentClient.GeneratePdfDocument(documentTemplateId, orderId, languages, fileId);
            var orderDto = await GetOrderDto(orderId);
            var customerDto = await _customerClient.FindCustomerFromAccountId(orderDto.AccountId);
            var tenantDto = await _tenantClient.FindCurrentTenant();
            var to = string.Join(";", customerDto.GetSuggestedEmailEntries().Select(x => x.Email).ToList());
            var fromName = tenantDto.CompanyName;
            var content = BuildEmailContent(fileId, fromName);
            var sendEmailDto = new SendEmailDto
            {
                To = to,
                FromName = fromName,
                Subject = documentTemplate.Name + " - Booking " + orderDto.FriendlyId.DefaultIdPadding() + " - " + fromName,
                Content = content,
                CustomerId = customerDto.Id,
                FriendlyCustomerId = customerDto.FriendlyId,
                OrderId = orderDto.Id,
                FriendlyOrderId = orderDto.FriendlyId,
                CustomerSearch = customerDto.DisplayString,
                Attachments = new List<AttachmentDto>
                {
                    new()
                    {
                        FileId = fileId,
                        FileName = documentTemplate.Name,
                        Type = "pdf"
                    }
                }
            };

            AddSmsRecipientsToEmailDto(forcedCustomerPhone, customerDto, sendEmailDto);
            await _documentClient.SendHtmlEmail(sendEmailDto);

            return fileId;
        }

        public async Task<Guid> SendEmailForOrderWithAlreadyCreatedDocument(string orderId, Guid fileId, string documentTemplateName)
        {
            var orderDto = await GetOrderDto(orderId);
            var customerDto = await _customerClient.FindCustomerFromAccountId(orderDto.AccountId);
            var tenantDto = await _tenantClient.FindCurrentTenant();
            var to = string.Join(";", customerDto.GetSuggestedEmailEntries().Select(x => x.Email).ToList());
            var fromName = tenantDto.CompanyName;
            var content = BuildEmailContent(fileId, fromName);
            var sendEmailDto = new SendEmailDto
            {
                To = to,
                FromName = fromName,
                Subject = documentTemplateName + " - Booking " + orderDto.FriendlyId.DefaultIdPadding() + " - " + fromName,
                Content = content,
                CustomerId = customerDto.Id,
                FriendlyCustomerId = customerDto.FriendlyId,
                OrderId = orderDto.Id,
                FriendlyOrderId = orderDto.FriendlyId,
                CustomerSearch = customerDto.DisplayString,
                Attachments = new List<AttachmentDto>
                {
                    new()
                    {
                        FileId = fileId,
                        FileName = documentTemplateName,
                        Type = "pdf"
                    }
                }
            };

            AddSmsRecipientsToEmailDto(null, customerDto, sendEmailDto);
            await _documentClient.SendHtmlEmail(sendEmailDto);

            return fileId;
        }

        private void AddSmsRecipientsToEmailDto(string forcedCustomerPhone, CustomerDto customerDto, SendEmailDto sendEmailDto)
        {
            if (_settingsRepository.SendSmsAfterOnlineBooking() && customerDto.Address.CountryId == "dk" &&
                customerDto.PhoneEntries.Count > 0)
            {
                if (forcedCustomerPhone == null)
                {
                    var phoneEntries = customerDto.GetSuggestedPhoneEntries();
                    var smsPdfLinkRecipients = string.Join(";", phoneEntries.Select(x => "+45" + x.Number));
                    sendEmailDto.SmsPdfLinkRecipients = smsPdfLinkRecipients;
                }
                else
                {
                    var smsPdfLinkRecipients = BuildValidDkPhone(forcedCustomerPhone);
                    sendEmailDto.SmsPdfLinkRecipients = smsPdfLinkRecipients;
                }
            }
        }

        private static string BuildValidDkPhone(string forcedCustomerPhone)
        {
            var smsPdfLinkRecipients =
                "+45" + forcedCustomerPhone.Replace("+45", "").Replace("0045", "").Replace(" ", "");
            return smsPdfLinkRecipients;
        }

        private async Task<Order> GetOrderDto(string orderId)
        {
            bool isFriendly = long.TryParse(orderId, out var friendlyId);
            Order orderDto = null;
            if (isFriendly)
            {
                orderDto = await _orderRepository.FindSingleFromFriendlyId(friendlyId, false);
            }
            else
            {
                orderDto = await _orderRepository.FindSingle(Guid.Parse(orderId), false);
            }

            return orderDto;
        }

        public async Task SendEmailConfirmationSms(string orderId, string phone, string fileId)
        {
            var message = await _messageClient.FindMostRecentEmailMessageForOrderId(orderId, MessageType.Email);
            var tenant = await _tenantClient.FindCurrentTenant();
            var companyName = tenant.CompanyName;
            var url = await _documentClient.ShortenUrl("https://nxbfilestorage.blob.core.windows.net/pdf/" + fileId);
            var content = "Link: " + url + " Mvh " + companyName;
            var recipientPhone = BuildValidDkPhone(phone);
            var sendSmsDto = new SendSmsDto()
            {
                To = recipientPhone,
                Content = content,
                CustomerId = message.CustomerId,
                FriendlyCustomerId = message.FriendlyCustomerId,
                OrderId = message.OrderId,
                FriendlyOrderId = message.FriendlyOrderId,
                CustomerSearch = message.CustomerSearch,
            };

            try
            {
                await _documentClient.SendSms(sendSmsDto);
            }
            catch (Exception exception)
            {
                _telemetry.TrackException(exception);
                _telemetry.TrackTrace("SendEmailConfirmationSms: Could not send sms to: " + recipientPhone + ": " + exception.Message, SeverityLevel.Information);
            }
        }

        private string BuildEmailContent(Guid fileId, string friendlyFromName)
        {
            string text = $@"
              <div>Vi fremsender hermed bekræftelse for deres booking. Det er vigtigt i kigger bekræftelsen igennem da den kan indeholde vigtige informationer gældende for jeres ophold ligesom den kan indeholde betingelser for afbestilling.</div>
              <div>Har i yderligere ønsker eller spørgsmål til jeres ophold beder vi jer kontakte os direkte ved at besvare denne e-mail eller telefon.</div>
              <br>
                <div>I kan også åbne den vedhæftede fil ved at klikke <a href=""{ BuildFileStorageHref(fileId)}"" target=""_new"">her</a></div>
                <br><br>
                <div>For at kunne åbne filer i PDF-format skal i bruge det gratis program Adobe® Reader®.<br> Du kan hente programmet her:<a href='https://get.adobe.com/reader/' target='_new'>Adobe® Reader®.</a></div>
                <br><br>
                <div>Med venlig hilsen</div>
                <br><br>
                <div>{friendlyFromName}</div>
                ";
            return text;
        }

        public string BuildFileStorageHref(Guid fileId, string folder = "pdf")
        {
            return $"https://nxbfilestorage.blob.core.windows.net/{folder}/{fileId.ToString()}";
        }
    }
}
