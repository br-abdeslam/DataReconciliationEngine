namespace DataReconciliationEngine.Application.DTOs;

/// <summary>
/// Filter, sort, and paging parameters for the results page.
/// Matches MudTable's ServerData callback shape.
/// </summary>
public sealed class ResultFilterDto
{
    public required int RunId { get; init; }

    /// <summary>Search by MatchKeyValue (Werfcode). Null = no filter.</summary>
    public string? SearchKey { get; init; }

    /// <summary>Filter by LogicalFieldName. Null or empty = all fields.</summary>
    public string? FieldName { get; init; }

    /// <summary>Show only missing records (_MISSING_).</summary>
    public bool OnlyMissing { get; init; }

    /// <summary>Show only mismatches (excludes full matches, if any were stored).</summary>
    public bool OnlyMismatches { get; init; } = true;

    // Paging
    public int Page { get; init; }
    public int PageSize { get; init; } = 25;

    // Sorting
    public string? SortField { get; init; }
    public bool SortDescending { get; init; }
}