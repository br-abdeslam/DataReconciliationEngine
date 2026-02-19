using System.ComponentModel.DataAnnotations;

namespace DataReconciliationEngine.Application.DTOs;

/// <summary>
/// DTO for creating or updating a field mapping.
/// Used by MudForm for validation.
/// </summary>
public sealed class FieldMappingEditDto
{
    [Required(ErrorMessage = "Logical field name is required.")]
    [MaxLength(100, ErrorMessage = "Logical field name cannot exceed 100 characters.")]
    public string LogicalFieldName { get; set; } = string.Empty;

    [Required(ErrorMessage = "System A column is required.")]
    [MaxLength(128, ErrorMessage = "System A column cannot exceed 128 characters.")]
    public string SystemA_Column { get; set; } = string.Empty;

    [Required(ErrorMessage = "System B column is required.")]
    [MaxLength(128, ErrorMessage = "System B column cannot exceed 128 characters.")]
    public string SystemB_Column { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}