
using DataReconciliationEngine.Application.Interfaces;
using DataReconciliationEngine.Domain.Entities;
using DataReconciliationEngine.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace DataReconciliationEngine.Web.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ReconciliationDbContext>();

        // Igration
        await db.Database.MigrateAsync();

        // 1) Seed the configuration if it does not exist yet ! 
        var comparisonName = "Company vs MMg_Sites (Werfcode)";

        var existingConfig = await db.TableComparisonConfigurations
            .FirstOrDefaultAsync(x => x.ComparisonName == comparisonName);

        if (existingConfig is null)
        {
            var config = new TableComparisonConfiguration
            {
                ComparisonName = comparisonName,
                SystemA_Table = "[PRO_BE01].[dbo].[Company]",
                SystemB_Table = "[Company].[dbo].[MMg_Sites]",
                MatchColumn_SystemA = "Werfcode",
                MatchColumn_SystemB = "Werfcode",
                IsActive = true
            };

            db.TableComparisonConfigurations.Add(config);
            await db.SaveChangesAsync();

            // 2) Seed Mapping
            var mappings = new List<FieldMappingConfiguration>
            {
                new()
                {
                    ComparisonConfigId = config.Id,
                    LogicalFieldName = "Name",
                    SystemA_Column = "Roepnaam",
                    SystemB_Column = "CallingName",
                    IsActive = true
                },
                new()
                {
                    ComparisonConfigId = config.Id,
                    LogicalFieldName = "Active",
                    SystemA_Column = "actief",
                    SystemB_Column = "Active",
                    IsActive = true
                },
                new()
                {
                    ComparisonConfigId = config.Id,
                    LogicalFieldName = "Department",
                    SystemA_Column = "Afdeling",
                    SystemB_Column = "Department",
                    IsActive = true
                },
                new()
                {
                    ComparisonConfigId = config.Id,
                    LogicalFieldName = "Company_Code",
                    SystemA_Column = "Firma",
                    SystemB_Column = "Company_Code",
                    IsActive = true  
                },
                new()
                {
                    ComparisonConfigId = config.Id,
                    LogicalFieldName = "Adfinity_ID",
                    SystemA_Column = "Adfinity_ID",
                    SystemB_Column = "Adfinity_ID",
                    IsActive = false  // Different ID systems — not comparable
                },
                new()
                {
                    ComparisonConfigId = config.Id,
                    LogicalFieldName = "LastUpdated",
                    SystemA_Column = "Datum_Laatste_wijziging",
                    SystemB_Column = "Updated_at",
                    IsActive = false  // Timestamps always differ — noise for now
                },

                // Optionnel (adresse - formats différents)
                new()
                {
                    ComparisonConfigId = config.Id,
                    LogicalFieldName = "Address",
                    SystemA_Column = "Straat1",
                    SystemB_Column = "Adresinfo",
                    IsActive = true
                }
            };

            db.FieldMappingConfigurations.AddRange(mappings);
            await db.SaveChangesAsync();
        }
      
    }

}