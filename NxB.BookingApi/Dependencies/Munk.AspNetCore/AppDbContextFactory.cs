using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;

namespace Munk.AspNetCore
{
    public class AppDbContextFactory<TAppDbContext> : IAppDbContextFactory<TAppDbContext> where TAppDbContext : DbContext
    {
        private readonly TAppDbContext _appDbContext;

        public AppDbContextFactory(TAppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        
        public TAppDbContext Create(string inMemoryId)
        {
            var appDbContext = (TAppDbContext)Activator.CreateInstance(typeof(TAppDbContext),
                    new DbContextOptionsBuilder<TAppDbContext>().UseSqlServer(_appDbContext.Database.GetDbConnection())
                        .Options);
            return appDbContext;
        }
    }
}
