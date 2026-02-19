using DataReconciliationEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataReconciliationEngine.Infrastructure.Persistence.Configurations;

public class ComparisonResultConfiguration : IEntityTypeConfiguration<ComparisonResult>
{
    public void Configure(EntityTypeBuilder<ComparisonResult> builder)
    {
        builder.HasKey(r => r.Id);

        // ── Max lengths ────────────────────────────────────────
        // Werfcode is nvarchar(10) in source DBs — 50 gives headroom for other configs
        builder.Property(r => r.MatchKeyValue)
            .HasMaxLength(50)
            .IsRequired();

        // Field names: "Name", "Active", "_MISSING_" etc. — 100 is plenty
        builder.Property(r => r.LogicalFieldName)
            .HasMaxLength(100)
            .IsRequired();

        // Values can be long (addresses, descriptions) — cap at 500
        builder.Property(r => r.ValueSystemA).HasMaxLength(500);
        builder.Property(r => r.ValueSystemB).HasMaxLength(500);

        // ── Foreign key: RunId → ComparisonRuns ─────────────────
        builder.HasOne<ComparisonRun>()
            .WithMany()
            .HasForeignKey(r => r.RunId)
            .OnDelete(DeleteBehavior.Cascade); // delete run → delete its results

        // ── Foreign key: ComparisonConfigId → TableComparisonConfigurations ─
        builder.HasOne<TableComparisonConfiguration>()
            .WithMany()
            .HasForeignKey(r => r.ComparisonConfigId)
            .OnDelete(DeleteBehavior.Restrict); // don't cascade-delete config

        // ── Indexes (matched to query patterns in RunQueryService) ──

        // Primary query: WHERE RunId = @runId (every page load)
        // + sorting by MatchKeyValue (default sort)
        // This is the most important index — covers the base query + default ORDER BY
        // Also covers the detail drawer query (WHERE RunId = @runId AND MatchKeyValue = @key) since MatchKeyValue is included in the index
        builder.HasIndex(r => new { r.RunId, r.MatchKeyValue })
            .HasDatabaseName("IX_ComparisonResults_RunId_MatchKeyValue");

        // Filter: WHERE RunId = @runId AND LogicalFieldName = @field
        // Used by field dropdown filter + "Missing only" toggle (_MISSING_)
        // Also covers GetFieldNamesForRunAsync (SELECT DISTINCT LogicalFieldName WHERE RunId = @runId) since LogicalFieldName is included in the index
        builder.HasIndex(r => new { r.RunId, r.LogicalFieldName })
            .HasDatabaseName("IX_ComparisonResults_RunId_LogicalFieldName");

        // Filter: WHERE RunId = @runId AND IsMatch = 0
        // Used by "Only mismatches" default filter
        builder.HasIndex(r => new { r.RunId, r.IsMatch })
            .HasDatabaseName("IX_ComparisonResults_RunId_IsMatch");

        // Detail drawer: WHERE RunId = @runId AND MatchKeyValue = @key
        // Already covered by IX_ComparisonResults_RunId_MatchKeyValue (exact match)

        // GetFieldNamesForRunAsync: SELECT DISTINCT LogicalFieldName WHERE RunId = @runId
        // Already covered by IX_ComparisonResults_RunId_LogicalFieldName
    }
}