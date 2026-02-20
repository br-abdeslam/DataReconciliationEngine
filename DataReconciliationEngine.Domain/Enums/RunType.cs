namespace DataReconciliationEngine.Domain.Enums
{
    /// <summary>
    /// Identifies the type of analysis performed by a run.
    /// Stored as int in ComparisonRuns.RunType.
    /// </summary>
    public enum RunType
    {
        /// <summary>Standard A-vs-B field comparison.</summary>
        Comparison = 1,

        /// <summary>Duplicate detection on Company.dbo.customer_sites.</summary>
        DuplicateCustomerSites = 2
    }
}