using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.OrderingApi
{
    public enum MatchOrderResult
    {
        None,
        NoMatch,
        MatchedNoCustomer,
        MatchedNotToday,
        MatchedToEarly,
        MatchedStillOccupied,
        MatchedAmountDue,
        MatchedNoErrors,
        MatchedAlreadyArrived,
        MatchedMissingLicensePlate,
    }
}
