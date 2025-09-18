using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NxB.Domain.Common.Enums;

namespace NxB.BookingApi.Models
{
    // TODO: Implement TConSqlBuilder interface - placeholder to fix compilation errors
    // This interface was referenced in query classes but not found in the codebase
    public interface ITConSqlBuilder
    {
        // TODO: Implement SQL builder methods for TCon queries
        string BuildWhereClauseRadioCodes(List<int> codes, TConRadioType radioType);
        Task<string> BuildWhereClauseTBDRadios();
        Task<string> BuildWhereClauseTBERadios();
    }
}