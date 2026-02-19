namespace DataReconciliationEngine.Application.DTOs;

/// <summary>
/// DTO for the run history list (last N runs).
/// </summary>
public sealed class RunSummaryDto
{
    public required int RunId { get; init; }
    public required int ComparisonConfigId { get; init; }
    public required string ComparisonName { get; init; }
    public required DateTime RunDate { get; init; }
    public required int TotalRecords { get; init; }
    public required int TotalMismatches { get; init; }
    public required int TotalMissingInA { get; init; }
    public required int TotalMissingInB { get; init; }
}