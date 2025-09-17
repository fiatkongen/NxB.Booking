using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NxB.BookingApi.Controllers.Tenant
{
    [Produces("application/json")]
    [Route("[controller]")]
    [AllowAnonymous]
    public class PingController : Controller
    {
        // GET api/values
        [HttpGet]
        public string Get()
        {
            var uniqueNodeId = Environment.GetEnvironmentVariable("Fabric_NodeIPOrFQDN");
            return uniqueNodeId;
        }
    }
}