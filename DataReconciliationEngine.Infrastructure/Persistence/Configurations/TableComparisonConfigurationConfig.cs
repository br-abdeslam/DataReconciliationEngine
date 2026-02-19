using DataReconciliationEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataReconciliationEngine.Infrastructure.Persistence.Configurations;

public class TableComparisonConfigurationConfig : IEntityTypeConfiguration<TableComparisonConfiguration>
{
    public void Configure(EntityTypeBuilder<TableComparisonConfiguration> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ComparisonName).HasMaxLength(200).IsRequired();
        builder.Property(c => c.SystemA_Table).HasMaxLength(300).IsRequired();
        builder.Property(c => c.SystemB_Table).HasMaxLength(300).IsRequired();
        builder.Property(c => c.MatchColumn_SystemA).HasMaxLength(128).IsRequired(); // SQL Server max identifier length
        builder.Property(c => c.MatchColumn_SystemB).HasMaxLength(128).IsRequired();

        // Unique constraint: no duplicate comparison names
        builder.HasIndex(c => c.ComparisonName)
            .IsUnique()
            .HasDatabaseName("IX_TableComparisonConfigs_ComparisonName");

        // Navigation: one config → many field mappings (already defined on FieldMapping side,
        // but explicitly declared here for clarity)
        builder.HasMany(c => c.FieldMappings)
            .WithOne(m => m.ComparisonConfig!)
            .HasForeignKey(m => m.ComparisonConfigId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}