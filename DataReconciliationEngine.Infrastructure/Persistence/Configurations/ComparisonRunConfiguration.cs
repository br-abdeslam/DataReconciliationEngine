using DataReconciliationEngine.Domain.Entities;
using DataReconciliationEngine.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataReconciliationEngine.Infrastructure.Persistence.Configurations;

public class ComparisonRunConfiguration : IEntityTypeConfiguration<ComparisonRun>
{
    public void Configure(EntityTypeBuilder<ComparisonRun> builder)
    {
        builder.HasKey(r => r.Id);

        // ── RunType (defaults to Comparison for all existing rows) ──
        builder.Property(r => r.RunType)
            .HasDefaultValue(RunType.Comparison)
            .IsRequired();

        // ── FK: now OPTIONAL (null for non-comparison run types) ──
        builder.HasOne<TableComparisonConfiguration>()
            .WithMany()
            .HasForeignKey(r => r.ComparisonConfigId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // ── Indexes ──
        builder.HasIndex(r => new { r.ComparisonConfigId, r.RunDate })
            .HasDatabaseName("IX_ComparisonRuns_ConfigId_RunDate")
            .IsDescending(false, true);

        builder.HasIndex(r => new { r.RunType, r.RunDate })
            .HasDatabaseName("IX_ComparisonRuns_RunType_RunDate")
            .IsDescending(false, true);
    }
}