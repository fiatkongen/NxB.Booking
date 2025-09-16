using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NxB.BookingApi.Models;
using NxB.Settings.Shared.Infrastructure;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    //https://docs.microsoft.com/en-us/ef/core/modeling/relationships
    //https://ardalis.com/encapsulated-collections-in-entity-framework-core
    public class AppDbContext : DbContext
    {
        // Login/User entities
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserTenantAccess> UserTenantAccess { get; set; }

        // Ordering entities
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }
        public DbSet<SubOrder> SubOrders { get; set; }

        // Accounting entities
        public DbSet<Account> Accounts { get; set; }

        // Inventory entities
        public DbSet<Article> Articles { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserSession>().ToTable("UserSession");
            modelBuilder.Entity<UserSession>().HasKey(x => x.SessionId);
            modelBuilder.Entity<UserSession>().Property(x => x.SessionId).HasMaxLength(50);
            modelBuilder.Entity<UserSession>().Property(x => x.UserJson).HasMaxLength(1000);

            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<User>().HasKey(x => x.Id);
            modelBuilder.Entity<User>().Property(x => x.Username).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<User>().Property(x => x.Password).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<User>().Property(x => x.Login).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<User>().Property(x => x.CountryId).HasMaxLength(2).IsRequired();
            modelBuilder.Entity<User>().Property(x => x.IsDisabled).IsRequired();
            modelBuilder.Entity<User>().Property(x => x.IsDeleted).IsRequired();
            modelBuilder.Entity<User>().HasMany(x => x.UserTenantAccesses).WithOne().HasForeignKey(x=> x.UserId);

            var navigation = modelBuilder.Entity<User>()
                .Metadata.FindNavigation(nameof(User.UserTenantAccesses));
            navigation.SetField("_userTenantAccesses");
            navigation.SetPropertyAccessMode(PropertyAccessMode.Field);

            modelBuilder.Entity<UserTenantAccess>().ToTable("UserTenantsAccess");
            modelBuilder.Entity<UserTenantAccess>().HasKey(x => new {x.TenantId, x.UserId});

            SharedSettingsDbContext.AddSettingsModelToContext(modelBuilder);
        }
    }
}
