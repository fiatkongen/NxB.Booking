using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Dto.AllocationApi;
using NxB.Clients.Interfaces;
using NxB.Dto.OrderingApi;

namespace NxB.Clients
{
    public class RentalSubTypeClient : NxBAdministratorClient, IRentalSubTypeClient
    {
        public RentalSubTypeClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task<RentalSubTypeDto> FindSingleOrDefault(Guid id)
        {
            var url = $"/NxB.Services.App/NxB.AllocationApi/rentalsubtype?id={id}";
            return await this.GetAsync<RentalSubTypeDto>(url);
        }
    }
}
