using System.ComponentModel.DataAnnotations;

namespace DataReconciliationEngine.Application.DTOs;

/// <summary>
/// DTO for creating or updating a comparison configuration.
/// Used by MudForm for validation.
/// </summary>
public sealed class ComparisonConfigEditDto
{
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
    public string ComparisonName { get; set; } = string.Empty;

    [Required(ErrorMessage = "System A table is required.")]
    [MaxLength(300, ErrorMessage = "System A table cannot exceed 300 characters.")]
    public string SystemA_Table { get; set; } = string.Empty;

    [Required(ErrorMessage = "System B table is required.")]
    [MaxLength(300, ErrorMessage = "System B table cannot exceed 300 characters.")]
    public string SystemB_Table { get; set; } = string.Empty;

    [Required(ErrorMessage = "Match column A is required.")]
    [MaxLength(128, ErrorMessage = "Match column A cannot exceed 128 characters.")]
    public string MatchColumn_SystemA { get; set; } = string.Empty;

    [Required(ErrorMessage = "Match column B is required.")]
    [MaxLength(128, ErrorMessage = "Match column B cannot exceed 128 characters.")]
    public string MatchColumn_SystemB { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}