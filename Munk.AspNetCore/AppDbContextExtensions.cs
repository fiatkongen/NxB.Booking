using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.EntityFrameworkCore
{
    public static class AppDbContextExtensions
    {
        public static TransactionScope CreateSnapshotTransaction(this DbContext dbContext)
        {
            return new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions {IsolationLevel = IsolationLevel.Snapshot},
                TransactionScopeAsyncFlowOption.Enabled);
        }
    }
}
