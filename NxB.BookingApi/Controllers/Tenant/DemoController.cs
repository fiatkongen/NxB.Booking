using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Controllers.Tenant
{
    [Produces("application/json")]
    [Authorize]
    [Route("demo")]
    [ApiValidationFilter]
    public class DemoController : Controller
    {
        private static string TALLYKEY_TENANTID = "CE88AC4D-2DE7-4F17-A58C-D7E4099A58E2";
        private readonly ITenantRepository _tenantRepository;

        public DemoController(ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        [HttpPost]
        [Route("delete/bookings/tallykey")]
        public IActionResult DeleteBookings()
        {
            _tenantRepository.DeleteBookings(Guid.Parse(TALLYKEY_TENANTID), false);
            return new OkResult();
        }
    }
}