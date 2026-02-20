namespace DataReconciliationEngine.Application.DTOs;

/// <summary>
/// Returned after a duplicate detection run completes.
/// </summary>
public sealed class DuplicateDetectionSummary
{
    public required int RunId { get; init; }
    public required int TotalRowsScanned { get; init; }
    public required int DuplicateGroupsFound { get; init; }
    public required int TotalDuplicateRecords { get; init; }
}