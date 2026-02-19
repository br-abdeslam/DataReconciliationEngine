namespace DataReconciliationEngine.Application.DTOs;

/// <summary>
/// Single row in the results MudTable.
/// </summary>
public sealed class ResultPageDto
{
    public required int Id { get; init; }
    public required string MatchKeyValue { get; init; }
    public required string LogicalFieldName { get; init; }
    public required string? ValueSystemA { get; init; }
    public required string? ValueSystemB { get; init; }
    public required bool ExistsInSystemA { get; init; }
    public required bool ExistsInSystemB { get; init; }
    public required bool IsMatch { get; init; }
    public required DateTime ComparedAt { get; init; }
}