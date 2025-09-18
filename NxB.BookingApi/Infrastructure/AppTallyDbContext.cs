using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class AppTallyDbContext : DbContext
    {
        public DbSet<TConRadio> TConRadios { get; set; }
        public DbSet<TConSocketTBB> TConTBBSockets { get; set; }
        public DbSet<TConSocketTWC> TConTWCSockets { get; set; }
        public DbSet<TConSocketTWEV> TConTWEVSockets { get; set; }
        public DbSet<TConTWCConsumption> TConTBE8Consumptions { get; set; }
        public DbSet<TConTBBConsumption> TConTBBConsumptions { get; set; }
        public DbSet<TConTWEVConsumption> TConTWEVConsumptions { get; set; }
        public DbSet<TConRadioAccessCode> TConRadioAccessCodes { get; set; }
        public DbSet<TConMasterRadio> TConMasterRadios { get; set; }
        public DbSet<TConStatusTBD> TConTBDStatuses { get; set; }
        public DbSet<TConTBDAccessLog> TConTBDAccessLogs { get; set; }
        public DbSet<TConTBDSetupPeriod> TConTBDSetupPeriods { get; set; }
        public DbSet<TConTBDSetupAccess> TConTBDSetupAccesses { get; set; }
        public DbSet<TConTWIStatus> TConTWIStatuses { get; set; }

        protected AppTallyDbContext(DbContextOptions options) : base(options) { }

        public AppTallyDbContext(TallyDbConnectionFactory tallyDbConnectionFactory) : base(new DbContextOptionsBuilder<AppTallyDbContext>().UseSqlServer(tallyDbConnectionFactory.Create(), sqlOptions => sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)).Options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TconRadio>().ToTable("Radio");
            modelBuilder.Entity<TconRadio>().HasKey(x => x.RadioAddr);
            modelBuilder.Entity<TconRadio>().Property(x => x.Type).HasColumnType("tinyint");

            modelBuilder.Entity<TConSocketTBB>().ToTable("TBBSocket");
            modelBuilder.Entity<TConSocketTBB>().HasKey(x => new { x.RadioAddr, x.SocketNo });

            modelBuilder.Entity<TConSocketTWC>().ToTable("TBE8Socket");
            modelBuilder.Entity<TConSocketTWC>().HasKey(x => new { x.RadioAddr, x.SocketNo });

            modelBuilder.Entity<TConTWCConsumption>().ToTable("TBE8Consumption");
            modelBuilder.Entity<TConTWCConsumption>().HasKey(x => x.Idx);

            modelBuilder.Entity<TConTBBConsumption>().ToTable("TBBConsumption");
            modelBuilder.Entity<TConTBBConsumption>().HasKey(x => x.Idx);

            modelBuilder.Entity<TConRadioAccessCode>().ToTable("RadioAccessCodes");
            modelBuilder.Entity<TConRadioAccessCode>().HasKey(x => x.Idx);

            modelBuilder.Entity<TConMasterRadio>().ToTable("MasterRadio");
            modelBuilder.Entity<TConMasterRadio>().HasKey(x => x.MasterAddr);

            modelBuilder.Entity<TConStatusTBD>().ToTable("TBD1status");
            modelBuilder.Entity<TConStatusTBD>().HasKey(x => x.RadioAddr);

            modelBuilder.Entity<TConTBDAccessLog>().ToTable("TBD1AccessLog");
            modelBuilder.Entity<TConTBDAccessLog>().HasKey(x => x.Idx);

            modelBuilder.Entity<TConTBDSetupPeriod>().ToTable("TBD1setupPeriod");
            modelBuilder.Entity<TConTBDSetupPeriod>().HasKey(x => new { x._MasterAddr, x._No });

            modelBuilder.Entity<TConTBDSetupAccess>().ToTable("TBD1setupAccess");
            modelBuilder.Entity<TConTBDSetupAccess>().HasKey(x => new { x._MasterAddr, x._No });

            modelBuilder.Entity<TConSocketTWEV>().ToTable("TWEVsocket");
            modelBuilder.Entity<TConSocketTWEV>().HasKey(x => new { x.RadioAddr, x.SocketNo });
            modelBuilder.Entity<TConSocketTWEV>().Property(x => x._Consumption).HasColumnName("Consumption");

            modelBuilder.Entity<TConTWEVConsumption>().ToTable("TWEVconsumption");
            modelBuilder.Entity<TConTWEVConsumption>().HasKey(x => x.Idx);

            modelBuilder.Entity<TConTWIStatus>().ToTable("TWIstatus");
            modelBuilder.Entity<TConTWIStatus>().HasKey(x => x.RadioAddr);
        }
    }
}