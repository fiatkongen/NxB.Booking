using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NxB.BookingApi.Models;

namespace NxB.Allocating.Shared.Infrastructure
{
    public class SharedDbContext
    {
        public static void AddSharedModelToContext(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Author>().ToTable("User");
            modelBuilder.Entity<Author>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<Author>().Property(x => x.Username).HasMaxLength(100).IsRequired();
        }
    }
}
