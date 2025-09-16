using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NxB.BookingApi.Models;
using NxB.BookingApi.Infrastructure;

namespace NxB.BookingApi.Controllers.Accounting
{
    [Produces("application/json")]
    [Route("friendlyidgenerator")]
    [Authorize]
    [ApiValidationFilter]
    public class FriendlyIdGeneratorController : BaseController
    {
        private readonly IFriendlyAccountingIdProvider _friendlyAccountingIdProvider;

        public FriendlyIdGeneratorController(IFriendlyAccountingIdProvider friendlyAccountingIdProvider)
        {
            _friendlyAccountingIdProvider = friendlyAccountingIdProvider;
        }


        [HttpGet]
        [Route("duedeposit")]
        public async Task<long> GenerateNextFriendlyDueDepositIdWithGapsAllowed()
        {
            var id = _friendlyAccountingIdProvider.GenerateNextFriendlyDueDepositId();
            return id;
        }
    }
}
