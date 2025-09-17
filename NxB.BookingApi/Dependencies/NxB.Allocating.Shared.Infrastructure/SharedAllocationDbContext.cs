using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using NxB.BookingApi.Models;

namespace NxB.Allocating.Shared.Infrastructure
{
    public class SharedAllocationDbContext : DbContext
    {
        public DbSet<AvailabilityMatrix> AvailabilityMatrices { get; set; }
        public DbSet<Allocation> Allocations { get; set; }

        public SharedAllocationDbContext(DbContextOptions<SharedAllocationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            AddAllocationModelToContext(modelBuilder);
        }

        public static void AddAllocationModelToContext(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AvailabilityMatrix>().ToTable("AvailabilityMatrix");
            modelBuilder.Entity<AvailabilityMatrix>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<AvailabilityMatrix>().Property(x => x.Id).HasMaxLength(100);
            modelBuilder.Entity<AvailabilityMatrix>().Property(x => x.TenantId).IsRequired();
            modelBuilder.Entity<AvailabilityMatrix>().Property(x => x.TenantId).IsRequired();
            modelBuilder.Entity<AvailabilityMatrix>().Ignore(x => x.AvailabilityArrays);

            modelBuilder.Entity<Allocation>().ToTable("Allocation");
            modelBuilder.Entity<Allocation>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<Allocation>().Property(x => x.TenantId).IsRequired();
            modelBuilder.Entity<Allocation>().Property(x => x.RentalUnitName).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Allocation>().Ignore(x => x.DateInterval);

            modelBuilder.Entity<RentalUnit>().ToTable("RentalUnit");
            modelBuilder.Entity<RentalUnit>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<RentalUnit>().Property(x => x.LegacyId).IsRequired();
            modelBuilder.Entity<RentalUnit>().Property(x => x.TenantId).IsRequired();
            modelBuilder.Entity<RentalUnit>().Ignore("Name");
            modelBuilder.Entity<RentalUnit>().Ignore(x => x.NameTranslator);
            modelBuilder.Entity<RentalUnit>().Ignore(x => x.DescriptionTranslator);

            modelBuilder.Entity<TimeSpanBase>().ToTable("TimeSpan");
            modelBuilder.Entity<TimeSpanBase>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<TimeSpanBase>().Property("Type").HasMaxLength(20);
            modelBuilder.Entity<TimeSpanBase>().HasIndex(x => new { x.TenantId });
            modelBuilder.Entity<TimeSpanBase>().Ignore(x => x.ResourceId);
            modelBuilder.Entity<TimeSpanBase>().Ignore(x => x.TimeBlock);
            modelBuilder.Entity<TimeSpanBase>().HasDiscriminator<string>("Type").HasValue<RentalCategoryTimeSpan>("rentalcategory").HasValue<TenantTimeSpan>("tenant");
            modelBuilder.Entity<TimeSpanBase>().Property(x => x.Sort).UseIdentityColumn().Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}
