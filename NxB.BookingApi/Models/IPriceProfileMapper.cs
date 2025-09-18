using System;
using System.Collections.Generic;

namespace NxB.BookingApi.Models
{
    public interface IPriceProfileMapper
    {
        Dictionary<string, Guid> Map();
    }
}