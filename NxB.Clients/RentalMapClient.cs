using NxB.Clients.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Dto.AllocationApi;
using NxB.Dto.LogApi;

namespace NxB.Clients
{
    public class RentalMapClient : NxBAdministratorClient, IRentalMapClient
    {
        public static string SERVICEURL = "/NxB.Services.App/NxB.AllocationApi";

        public RentalMapClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public Task<RentalMapDto> FindRentalMap(Guid id)
        {
            var url = $"{SERVICEURL}/rentalmap?id={id}";
            return this.GetAsync<RentalMapDto>(url);
        }

        public Task<RentalMapDto> FindFirstRentalMap()
        {
            var url = $"{SERVICEURL}/rentalmap/first";
            return this.GetAsync<RentalMapDto>(url);
        }
    }
}
