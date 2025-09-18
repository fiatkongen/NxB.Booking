using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NxB.BookingApi.Infrastructure;

namespace NxB.BookingApi.Controllers.TallyWebIntegration
{
    [Produces("application/json")]
    [Route("[controller]")]
    public class PingController : BaseController
    {
        // GET api/values
        [HttpGet]
        public string Get()
        {
            // TODO: Replace Service Fabric specific environment variable
            // var uniqueNodeId = Environment.GetEnvironmentVariable("Fabric_NodeIPOrFQDN");
            var uniqueNodeId = Environment.MachineName; // Placeholder replacement
            return uniqueNodeId;
        }
    }
}
