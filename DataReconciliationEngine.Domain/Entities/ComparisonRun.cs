using DataReconciliationEngine.Domain.Enums;

namespace DataReconciliationEngine.Domain.Entities
{
    public class ComparisonRun
    {
        public int Id { get; set; }

        /// <summary>Discriminator: Comparison (1) or DuplicateCustomerSites (2).</summary>
        public RunType RunType { get; set; } = RunType.Comparison;

        /// <summary>
        /// FK to TableComparisonConfigurations.
        /// Required for Comparison runs; null for DuplicateCustomerSites.
        /// </summary>
        public int? ComparisonConfigId { get; set; }

        public DateTime RunDate { get; set; } = DateTime.UtcNow;

        // Comparison:             TotalRecords=matched keys, TotalMismatches=keys with diffs
        // DuplicateCustomerSites: TotalRecords=source rows,  TotalMismatches=duplicate groups
        public int TotalRecords { get; set; }
        public int TotalMismatches { get; set; }
        public int TotalMissingInA { get; set; }
        public int TotalMissingInB { get; set; }
    }
}