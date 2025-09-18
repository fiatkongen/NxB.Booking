using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NxB.Domain.Common.Interfaces;

namespace Munk.AspNetCore
{
    public abstract class BaseAppDbContext<TAppDbContext> : DbContext where TAppDbContext : DbContext
    {
        private readonly TelemetryClient _telemetryClient;

        protected BaseAppDbContext(DbContextOptions<TAppDbContext> options, TelemetryClient telemetryClient) : base(options)
        {
            _telemetryClient = telemetryClient;
            this.ChangeTracker.Tracked += OnObjectTracked;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.LogTo(PerformLog);

        private void PerformLog(string logItem)
        {
            if (_telemetryClient == null)
            {
                return;
            }
            if (logItem.Contains("initialized 'AppDbContext'"))
            {
                LogDbConnections(true);
            }
            if (logItem.Contains("'AppDbContext' disposed"))
            {
                LogDbConnections(false);
            }
        }

        private void LogDbConnections(bool opened)
        {
            var metrics = new Dictionary<string, double> { { "AppDbContextCount", opened ? 1 : -1 } };
            _telemetryClient.TrackEvent("AppContext Metrics", metrics: metrics);  //too much logging
        }

        private void OnObjectTracked(object? sender, EntityTrackedEventArgs e)
        {
            if (e.Entry.Entity is IJsonEntity entity && e.Entry.State == EntityState.Unchanged)
            {
                entity.Deserialize();
            }

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            SaveChangesForEntities();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            SaveChangesForEntities();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
        {
            SaveChangesForEntities();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new())
        {
            SaveChangesForEntities();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void SaveChangesForEntities()
        {
            var entityEntries = this.ChangeTracker.Entries().Where(x => x.Entity is IEntitySaved).Select(x => new { entity = x.Entity as IEntitySaved, state = x.State }).ToList(); //always convert to json. Removed .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified), since sometimes the entity was modified, but not marked as modified (the matrix.list was not mapped to an entity int the current case)
            foreach (var entry in entityEntries)
            {
                entry.entity.OnEntitySaved(entry.state);
            }
        }

        protected static void ModifyDefaultCascadeBehavior(ModelBuilder modelBuilder, DeleteBehavior deleteBehavior)
        {
            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
                fk.DeleteBehavior = deleteBehavior;
        }
    }
}