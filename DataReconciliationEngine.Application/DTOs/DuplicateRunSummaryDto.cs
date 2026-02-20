namespace DataReconciliationEngine.Application.DTOs;

/// <summary>DTO for the duplicate detection run history list.</summary>
public sealed class DuplicateRunSummaryDto
{
    public required int RunId { get; init; }
    public required DateTime RunDate { get; init; }
    public required int TotalRowsScanned { get; init; }
    public required int DuplicateGroupsFound { get; init; }
    public required int TotalDuplicateRecords { get; init; }
}