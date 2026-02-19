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

        public DbSet<TableComparisonConfiguration> TableComparisonConfigurations => Set<TableComparisonConfiguration>();
        public DbSet<FieldMappingConfiguration> FieldMappingConfigurations => Set<FieldMappingConfiguration>();
        public DbSet<ComparisonRun> ComparisonRuns => Set<ComparisonRun>();
        public DbSet<ComparisonResult> ComparisonResults => Set<ComparisonResult>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Applique toutes les configurations EF Core (si tu en crées)
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReconciliationDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
