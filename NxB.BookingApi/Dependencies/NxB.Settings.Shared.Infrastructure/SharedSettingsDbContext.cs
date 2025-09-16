using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace NxB.Settings.Shared.Infrastructure
{
    public class SharedSettingsDbContext
    {
        public static void AddSettingsModelToContext(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SettingsItem>().ToTable("Settings");
            modelBuilder.Entity<SettingsItem>().HasKey(x => new{ x.Id, x.Context});
            modelBuilder.Entity<SettingsItem>().Property(x => x.Context).HasMaxLength(20).IsRequired();
            modelBuilder.Entity<SettingsItem>().Property(x => x.JsonSettingsItem).HasColumnType("ntext");
            modelBuilder.Entity<SettingsItem>().Ignore(x => x.Value);
        }
    }
}
