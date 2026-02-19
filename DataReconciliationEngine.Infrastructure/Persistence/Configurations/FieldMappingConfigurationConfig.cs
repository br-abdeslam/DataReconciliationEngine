using DataReconciliationEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataReconciliationEngine.Infrastructure.Persistence.Configurations;

public class FieldMappingConfigurationConfig : IEntityTypeConfiguration<FieldMappingConfiguration>
{
    public void Configure(EntityTypeBuilder<FieldMappingConfiguration> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.LogicalFieldName).HasMaxLength(100).IsRequired();
        builder.Property(m => m.SystemA_Column).HasMaxLength(128).IsRequired(); // SQL Server max identifier
        builder.Property(m => m.SystemB_Column).HasMaxLength(128).IsRequired();

        // FK is already configured in TableComparisonConfigurationConfig.
        // Index on ComparisonConfigId is auto-created by EF for the FK.
    }
}