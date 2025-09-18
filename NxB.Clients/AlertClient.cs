using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Domain.Common.Interfaces;
using NxB.Clients.Interfaces;
using ServiceStack;

namespace NxB.Clients
{
    public class AlertClient : NxBAdministratorClient, IAlertClient
    {
        public async Task SendSmsToSupport(string text)
        {
            await Call(async () =>
            {
                var url = $"/NxB.Services.App/NxB.DocumentApi/sms/send/sms/support?text=" + WebUtility.UrlEncode(text); 
                await this.PostAsync(url, null);
            });
        }

        public AlertClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }
    }
}
