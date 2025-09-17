using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.BookingApi.Models;
using ServiceStack;

namespace NxB.BookingApi.Infrastructure
{
    public class RentalCachesClient : NxBAdministratorClient, IRentalCachesClient
    {
        public RentalCachesClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task Initialize(DateTime start, DateTime end)
        {
            var url = $"/NxB.Services.App/NxB.AllocationApi/availability/rentalcaches/initialize?start={start.ToJsonDateString()}&end={end.ToJsonDateString()}";
            await this.GetAsync<string>(url);
        }

        public bool IsTestClient => false;
    }
}
