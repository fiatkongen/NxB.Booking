using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using NxB.Domain.Common.Interfaces;

namespace Munk.AspNetCore
{
    [Serializable]
    public class Counter : ITenantEntity
    {
        public Guid TenantId { get; set; }
        public string Id { get; set; }
        public string Count { get; set; }

        public static void AddCounterModelToContext(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Counter>().ToTable("Counter");
            modelBuilder.Entity<Counter>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<Counter>().Property(x => x.Id).HasMaxLength(50);
            modelBuilder.Entity<Counter>().Property(x => x.Count).HasMaxLength(100);
        }
    }

    public class CounterIdProvider<TAppDbContext> : ICounterIdProvider where TAppDbContext : DbContext
    {
        private readonly TAppDbContext _appDbContext;
        private readonly IClaimsProvider _claimsProvider;

        public CounterIdProvider(TAppDbContext appDbContext, IClaimsProvider claimsProvider)
        {
            _appDbContext = appDbContext;
            _claimsProvider = claimsProvider;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public long Next(string id, long seed = 0)
        {
            long count = seed;

            var counter = _appDbContext.Set<Counter>().SingleOrDefault(x => x.Id == id && x.TenantId == _claimsProvider.GetTenantId());
            if (counter == null)
            {
                counter = new Counter() { Id = id, TenantId = _claimsProvider.GetTenantId(), Count = count.ToString() };
                _appDbContext.Add(counter);
            }
            else
            {
                count = long.Parse(counter.Count);
                count++;
                counter.Count = count.ToString();
            }
            return count;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public long Next_Shared(string id, long seed = 0)
        {
            long count = seed;

            var counter = _appDbContext.Set<Counter>().SingleOrDefault(x => x.Id == id && x.TenantId == Guid.Empty);
            if (counter == null)
            {
                counter = new Counter() { Id = id, TenantId = Guid.Empty, Count = count.ToString() };
                _appDbContext.Add(counter);
            }
            else
            {
                count = long.Parse(counter.Count);
                count++;
                counter.Count = count.ToString();
            }
            return count;
        }

        public long Get(string id, long seed = 0, Guid? tenantId = null)
        {
            long count = seed;

            var counter = _appDbContext.Set<Counter>().SingleOrDefault(x => x.Id == id);
            if (counter == null)
            {
                counter = new Counter() { Id = id, TenantId = tenantId ?? _claimsProvider.GetTenantId(), Count = count.ToString() };
                _appDbContext.Add(counter);
            }
            else
            {
                count = long.Parse(counter.Count);
                counter.Count = count.ToString();
            }
            return count;
        }

        public void Set(string id, long count, Guid? tenantId = null)
        {
            var counter = _appDbContext.Set<Counter>().SingleOrDefault(x => x.Id == id);
            if (counter == null)
            {
                counter = new Counter() { Id = id, TenantId = tenantId ?? _claimsProvider.GetTenantId(), Count = count.ToString() };
                _appDbContext.Add(counter);
            }
            counter.Count = count.ToString();
        }
    }
}