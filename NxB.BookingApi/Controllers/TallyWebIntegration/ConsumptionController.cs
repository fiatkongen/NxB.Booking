using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using NxB.BookingApi.Infrastructure;


namespace NxB.BookingApi.Controllers.TallyWebIntegration
{
    [Produces("application/json")]
    [Route("consumption")]
    [Authorize]
    [ApiValidationFilter]
    public class ConsumptionController : BaseController
    {
        private readonly CalculateConsumptionsQuery _calculateConsumptionsQuery;
        private readonly CalculateUnsettledConsumptionsQuery _calculateUnsettledConsumptionsQuery;

        public ConsumptionController(CalculateConsumptionsQuery calculateConsumptionsQuery, CalculateUnsettledConsumptionsQuery calculateUnsettledConsumptionsQuery)
        {
            _calculateConsumptionsQuery = calculateConsumptionsQuery;
            _calculateUnsettledConsumptionsQuery = calculateUnsettledConsumptionsQuery;
        }

        [HttpGet]
        [Route("calculate")]
        public async Task<ObjectResult> CalculateConsumptions(uint? code, DateTime filterFrom, DateTime? filterTo = null)
        {
            var consumptionTotalDtos = await _calculateConsumptionsQuery.ExecuteAsync((int)code, filterFrom, filterTo);
            return new OkObjectResult(consumptionTotalDtos);
        }


        [HttpGet]
        [Route("calculate/unsettled")]
        public async Task<ObjectResult> CalculateUnsettledConsumptions(string codes)
        {
            var codesInt = codes.Split(',').Select(x => (int)uint.Parse(x)).ToList();
            var consumptionTotalDtos = await _calculateUnsettledConsumptionsQuery.ExecuteAsync(codesInt);
            return new OkObjectResult(consumptionTotalDtos);
        }
    }
}
