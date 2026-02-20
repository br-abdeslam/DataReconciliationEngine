using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataReconciliationEngine.Domain.Entities;

namespace DataReconciliationEngine.Infrastructure.Persistence.Contexts
{
    public class ReconciliationDbContext : DbContext
    {
        public ReconciliationDbContext(DbContextOptions<ReconciliationDbContext> options) : base(options) { }

        // ── Comparison ──
        public DbSet<TableComparisonConfiguration> TableComparisonConfigurations => Set<TableComparisonConfiguration>();
        public DbSet<FieldMappingConfiguration> FieldMappingConfigurations => Set<FieldMappingConfiguration>();
        public DbSet<ComparisonRun> ComparisonRuns => Set<ComparisonRun>();
        public DbSet<ComparisonResult> ComparisonResults => Set<ComparisonResult>();

        // ── Duplicate detection ──
        public DbSet<DuplicateGroup> DuplicateGroups => Set<DuplicateGroup>();
        public DbSet<DuplicateRecord> DuplicateRecords => Set<DuplicateRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReconciliationDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}