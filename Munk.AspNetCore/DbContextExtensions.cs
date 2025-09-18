using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.EntityFrameworkCore
{
    public static class DbContextExtensions
    {
        public static void ClearDbContextCache(this DbContext dbContext)
        {
            var entityEntries = dbContext.ChangeTracker.Entries().ToList();
            foreach (var entity in entityEntries)
            {
                entity.State = EntityState.Detached;
            }
        }
    }
}
