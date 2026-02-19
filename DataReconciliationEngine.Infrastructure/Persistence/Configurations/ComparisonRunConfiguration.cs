using DataReconciliationEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataReconciliationEngine.Infrastructure.Persistence.Configurations;

public class ComparisonRunConfiguration : IEntityTypeConfiguration<ComparisonRun>
{
    public void Configure(EntityTypeBuilder<ComparisonRun> builder)
    {
        builder.HasKey(r => r.Id);

        // ── Foreign key: ComparisonConfigId → TableComparisonConfigurations ─
        builder.HasOne<TableComparisonConfiguration>()
            .WithMany()
            .HasForeignKey(r => r.ComparisonConfigId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Index: GetLastRunsAsync filters by ConfigId, sorts by RunDate DESC ─
        // This covers: WHERE ComparisonConfigId = @id ORDER BY RunDate DESC
        builder.HasIndex(r => new { r.ComparisonConfigId, r.RunDate })
            .HasDatabaseName("IX_ComparisonRuns_ConfigId_RunDate")
            .IsDescending(false, true); // ConfigId ASC, RunDate DESC
    }
}