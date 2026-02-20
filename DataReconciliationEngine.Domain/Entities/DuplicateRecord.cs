namespace DataReconciliationEngine.Domain.Entities
{
    /// <summary>
    /// One customer_sites row inside a <see cref="DuplicateGroup"/>,
    /// with both raw source values and normalized values for comparison.
    /// </summary>
    public class DuplicateRecord
    {
        public int Id { get; set; }

        /// <summary>FK to DuplicateGroups.</summary>
        public int DuplicateGroupId { get; set; }

        /// <summary>Source PK: customer_sites.Customer_Sites_ID.</summary>
        public int CustomerSitesId { get; set; }

        // ── Raw values (as-is from source) ──
        public string? StreetRaw { get; set; }
        public string? NumberRaw { get; set; }
        public string? BoxRaw { get; set; }
        public string? ZipRaw { get; set; }
        public string? CityRaw { get; set; }

        // ── Normalized values (trimmed, uppercased, Number/Box parsed) ──
        public string? StreetNorm { get; set; }
        public string? NumberNorm { get; set; }
        public string? BoxNorm { get; set; }
        public string? ZipNorm { get; set; }
        public string? CityNorm { get; set; }

        // ── Coordinates (full precision from source) ──
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        /// <summary>
        /// Quality score: higher = more complete/trusted record.
        /// Used to pick the master suggestion per group.
        /// </summary>
        public int CompletenessScore { get; set; }

        /// <summary>True for the single best record in the group (highest score, oldest tie-breaker).</summary>
        public bool IsMasterSuggested { get; set; }

        /// <summary>Optional explanation, e.g. "Highest score + oldest".</summary>
        public string? Reason { get; set; }
    }
}