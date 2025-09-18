using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Clients.Interfaces;
using NxB.Dto.DocumentApi;
using ServiceStack;

namespace NxB.Clients
{
    public class FriendlyIdGeneratorClient : NxBAdministratorClient, IFriendlyIdGeneratorClient
    {
        public static string SERVICEURL = "/NxB.Services.App/NxB.AccountingApi";

        public FriendlyIdGeneratorClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }
        
        public async Task<long> GenerateNextFriendlyDueDepositId()
        {
            var url = $"{SERVICEURL}/friendlyidgenerator/duedeposit";
            var id = await this.GetAsync<long>(url);
            return id;
        }
   }
}
