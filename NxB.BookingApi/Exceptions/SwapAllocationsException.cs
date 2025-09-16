using System;
using System.Collections.Generic;
using System.Text;
using NxB.Dto.OrderingApi;

namespace NxB.BookingApi.Exceptions
{
    public class SwapAllocationsException : Exception
    {
        //potential hack to have a dto as parameter
        public SwapAllocationsException(SwapAllocationsDto dto, string friendlyRentalUnitId1, string friendlyRentalUnitId2, string extraUserMessage = "")
            : base(
                $"Fejl ved ombytning af enheder {friendlyRentalUnitId1} og {friendlyRentalUnitId2}. {extraUserMessage}")
        {
        }
    }
}
