namespace DataReconciliationEngine.Domain.Entities
{
    /// <summary>
    /// One cluster of customer_sites rows that share the same
    /// (Group_ID, rounded Latitude, rounded Longitude) key.
    /// </summary>
    public class DuplicateGroup
    {
        public int Id { get; set; }

        /// <summary>FK to ComparisonRuns (RunType = DuplicateCustomerSites).</summary>
        public int RunId { get; set; }

        /// <summary>customer_sites.Group_ID (uniqueidentifier, nullable in source).</summary>
        public Guid? GroupId { get; set; }

        /// <summary>ROUND(Latitude, 6) — used for grouping.</summary>
        public decimal? LatRound { get; set; }

        /// <summary>ROUND(Longitude, 6) — used for grouping.</summary>
        public decimal? LonRound { get; set; }

        /// <summary>Composite text key, e.g. "3F2504E0-...|50.846700|4.357200".</summary>
        public string CandidateKey { get; set; } = string.Empty;

        /// <summary>Total rows in this group (always ≥ 2).</summary>
        public int RecordsCount { get; set; }

        /// <summary>Reserved — rows with exact normalized address match.</summary>
        public int CertainCount { get; set; }

        /// <summary>Reserved — rows with close but not identical match.</summary>
        public int ProbableCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation ──
        public ICollection<DuplicateRecord> Records { get; set; } = new List<DuplicateRecord>();
    }
}