using DataReconciliationEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataReconciliationEngine.Infrastructure.Persistence.Configurations;

public class DuplicateRecordConfiguration : IEntityTypeConfiguration<DuplicateRecord>
{
    public void Configure(EntityTypeBuilder<DuplicateRecord> builder)
    {
        builder.HasKey(r => r.Id);

        // ── Raw columns ─────────────────────────────────────────
        builder.Property(r => r.StreetRaw).HasMaxLength(200);
        builder.Property(r => r.NumberRaw).HasMaxLength(50);
        builder.Property(r => r.BoxRaw).HasMaxLength(50);
        builder.Property(r => r.ZipRaw).HasMaxLength(20);
        builder.Property(r => r.CityRaw).HasMaxLength(150);

        // ── Normalized columns ──────────────────────────────────
        builder.Property(r => r.StreetNorm).HasMaxLength(200);
        builder.Property(r => r.NumberNorm).HasMaxLength(50);
        builder.Property(r => r.BoxNorm).HasMaxLength(50);
        builder.Property(r => r.ZipNorm).HasMaxLength(20);
        builder.Property(r => r.CityNorm).HasMaxLength(150);

        // ── Coordinates (full precision from source) ────────────
        builder.Property(r => r.Latitude).HasPrecision(17, 14);
        builder.Property(r => r.Longitude).HasPrecision(17, 14);

        // ── Reason ──────────────────────────────────────────────
        builder.Property(r => r.Reason).HasMaxLength(200);

        // ── Indexes ─────────────────────────────────────────────
        // Detail drawer: load all records for a group
        builder.HasIndex(r => r.DuplicateGroupId)
            .HasDatabaseName("IX_DuplicateRecords_DuplicateGroupId");

        // Cross-run lookup: "show me every run where this site appeared as duplicate"
        builder.HasIndex(r => r.CustomerSitesId)
            .HasDatabaseName("IX_DuplicateRecords_CustomerSitesId");
    }
}