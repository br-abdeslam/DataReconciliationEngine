namespace DataReconciliationEngine.Application.DTOs;

/// <summary>
/// DTO for displaying a field mapping in lists.
/// </summary>
public sealed class FieldMappingDto
{
    public required int Id { get; init; }
    public required int ComparisonConfigId { get; init; }
    public required string LogicalFieldName { get; init; }
    public required string SystemA_Column { get; init; }
    public required string SystemB_Column { get; init; }
    public required bool IsActive { get; init; }
}