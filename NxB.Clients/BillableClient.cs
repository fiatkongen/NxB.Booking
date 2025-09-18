using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.Clients.Interfaces;
using NxB.Dto.DocumentApi;
using NxB.Dto.TenantApi;
using ServiceStack;

namespace NxB.Clients
{
    public class BillableClient : NxBAdministratorClient, IBillableClient
    {
        public static string SERVICEURL = "/NxB.Services.App/NxB.TenantApi";

        public BillableClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            
        }

        public async Task<BillableItemDto> FindSingle(Guid id)
        {
            var url = $"{SERVICEURL}/billing?id={id}";
            return await this.GetAsync<BillableItemDto>(url);
        }

        public async Task<BillableItemDto> FindSingleFromBillableItemRef(Guid billedItemRef)
        {
            var url = $"{SERVICEURL}/billing/billeditemref?billedItemRef={billedItemRef}";
            return await this.GetAsync<BillableItemDto>(url);
        }

        public async Task ActivateItem(Guid billedItemRef)
        {
            var url = $"{SERVICEURL}/billing/activate?billedItemRef={billedItemRef}";
            await this.PutAsync(url, null);
        }

        public async Task SetDeliveryStatus(Guid billedItemRef, DeliveryStatus deliveryStatus)
        {
            var url = $"{SERVICEURL}/billing/deliverystatus?billedItemRef={billedItemRef}&deliveryStatus={deliveryStatus}";
            await this.PutAsync(url, null);
        }

        public async Task DeleteItem(Guid billedItemRef)
        {
            var url = $"{SERVICEURL}/billing/delete?billedItemRef={billedItemRef}";
            await this.DeleteAsync(url);
        }

        public async Task TryDeleteItem(Guid billedItemRef)
        {
            try
            {
                await this.DeleteItem(billedItemRef);
            }
            catch {}
        }
    }
}
