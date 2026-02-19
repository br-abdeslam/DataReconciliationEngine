namespace DataReconciliationEngine.Application.DTOs;

/// <summary>
/// Parameters for exporting comparison results to CSV.
/// Mirrors the filter options on the Results page.
/// </summary>
public sealed class ExportRequestDto
{
    public required int RunId { get; init; }

    /// <summary>Filter by MatchKeyValue substring. Null = no filter.</summary>
    public string? SearchKey { get; init; }

    /// <summary>Filter by LogicalFieldName. Null = all fields.</summary>
    public string? FieldName { get; init; }

    /// <summary>Show only missing records (_MISSING_).</summary>
    public bool OnlyMissing { get; init; }

    /// <summary>Show only mismatches (IsMatch = false, excluding _MISSING_).</summary>
    public bool OnlyMismatches { get; init; }

    /// <summary>
    /// Label used in the file name to indicate filter type.
    /// Examples: "All", "Filtered", "Missing", "Mismatches"
    /// </summary>
    public string FileLabel { get; init; } = "All";
}