using DataReconciliationEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataReconciliationEngine.Infrastructure.Persistence.Configurations;

public class DuplicateGroupConfiguration : IEntityTypeConfiguration<DuplicateGroup>
{
    public void Configure(EntityTypeBuilder<DuplicateGroup> builder)
    {
        builder.HasKey(g => g.Id);

        // ── FK: RunId → ComparisonRuns (cascade: purge run → delete groups) ──
        builder.HasOne<ComparisonRun>()
            .WithMany()
            .HasForeignKey(g => g.RunId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Properties ──────────────────────────────────────────
        builder.Property(g => g.LatRound).HasPrecision(9, 6);
        builder.Property(g => g.LonRound).HasPrecision(9, 6);

        builder.Property(g => g.CandidateKey)
            .HasMaxLength(200)
            .IsRequired();

        // ── Navigation: one group → many records ────────────────
        builder.HasMany(g => g.Records)
            .WithOne()
            .HasForeignKey(r => r.DuplicateGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Indexes (matched to upcoming query patterns) ────────
        // Lookup by run + candidate key (uniqueness check during insert)
        builder.HasIndex(g => new { g.RunId, g.CandidateKey })
            .HasDatabaseName("IX_DuplicateGroups_RunId_CandidateKey");

        // Results page: ORDER BY RecordsCount DESC within a run
        builder.HasIndex(g => new { g.RunId, g.RecordsCount })
            .HasDatabaseName("IX_DuplicateGroups_RunId_RecordsCount")
            .IsDescending(false, true);
    }
}