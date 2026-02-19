namespace DataReconciliationEngine.Application.DTOs;

/// <summary>
/// DTO for displaying a comparison configuration in lists and detail views.
/// </summary>
public sealed class ComparisonConfigDto
{
    public required int Id { get; init; }
    public required string ComparisonName { get; init; }
    public required string SystemA_Table { get; init; }
    public required string SystemB_Table { get; init; }
    public required string MatchColumn_SystemA { get; init; }
    public required string MatchColumn_SystemB { get; init; }
    public required bool IsActive { get; init; }
    public required int FieldMappingCount { get; init; }
    public required int RunCount { get; init; }
}