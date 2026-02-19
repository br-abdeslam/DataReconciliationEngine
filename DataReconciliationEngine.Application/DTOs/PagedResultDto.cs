namespace DataReconciliationEngine.Application.DTOs;

/// <summary>
/// Generic paged response wrapper — used by MudTable's ServerData callback.
/// </summary>
public sealed class PagedResultDto<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int TotalCount { get; init; }
}