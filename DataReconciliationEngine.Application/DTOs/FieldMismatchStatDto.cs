namespace DataReconciliationEngine.Application.DTOs;

/// <summary>
/// Aggregation: mismatch count grouped by LogicalFieldName.
/// Used for "Top mismatched fields" analytics.
/// </summary>
public sealed class FieldMismatchStatDto
{
    public required string LogicalFieldName { get; init; }
    public required int Count { get; init; }
}