using System;
using System.Threading.Tasks;

namespace NxB.BookingApi.Infrastructure
{
    public interface IOrderCommunicationHelper
    {
        Task<Guid> SendEmailForOrder(string orderId, Guid documentTemplateId, string languages, string forcedCustomerPhone = null);
        Task<Guid> SendEmailForOrderWithAlreadyCreatedDocument(string orderId, Guid fileId, string documentTemplateName);
        Task SendEmailConfirmationSms(string orderId, string phone, string fileId);
    }
}
