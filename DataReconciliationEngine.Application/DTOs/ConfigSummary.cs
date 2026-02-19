namespace DataReconciliationEngine.Application.DTOs;

/// <summary>
/// Lightweight DTO for populating the configuration dropdown.
/// </summary>
public sealed class ConfigSummaryDto
{
    public required int Id { get; init; }
    public required string ComparisonName { get; init; }
    public required bool IsActive { get; init; }
}