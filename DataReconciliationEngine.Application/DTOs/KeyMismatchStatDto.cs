namespace DataReconciliationEngine.Application.DTOs;

/// <summary>
/// Aggregation: mismatch count grouped by MatchKeyValue (Werfcode).
/// Used for "Top problematic sites" analytics.
/// </summary>
public sealed class KeyMismatchStatDto
{
    public required string MatchKeyValue { get; init; }
    public required int Count { get; init; }
}