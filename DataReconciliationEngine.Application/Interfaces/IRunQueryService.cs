using DataReconciliationEngine.Application.DTOs;

namespace DataReconciliationEngine.Application.Interfaces;

/// <summary>
/// Read-only query service for the UI. All queries hit the local DB only.
/// </summary>
public interface IRunQueryService
{
    /// <summary>Returns all active comparison configurations for the dropdown.</summary>
    Task<List<ConfigSummaryDto>> GetActiveConfigurationsAsync(CancellationToken ct = default);

    /// <summary>Returns the last <paramref name="take"/> runs for a given config.</summary>
    Task<List<RunSummaryDto>> GetLastRunsAsync(int configId, int take = 10, CancellationToken ct = default);

    /// <summary>Returns the summary for a single run.</summary>
    Task<RunSummaryDto?> GetRunSummaryAsync(int runId, CancellationToken ct = default);

    /// <summary>Returns a paged, filtered, sorted list of comparison results.</summary>
    Task<PagedResultDto<ResultPageDto>> GetResultsPageAsync(ResultFilterDto filter, CancellationToken ct = default);

    /// <summary>Returns all result rows for a given run + match key (for the detail dialog).</summary>
    Task<List<ResultPageDto>> GetResultsByKeyAsync(int runId, string matchKeyValue, CancellationToken ct = default);

    /// <summary>Returns distinct LogicalFieldName values for a run (for the field dropdown filter).</summary>
    Task<List<string>> GetFieldNamesForRunAsync(int runId, CancellationToken ct = default);

    /// <summary>Deletes a run and all its results (cascade). Returns true if the run existed.</summary>
    Task<bool> DeleteRunAsync(int runId, CancellationToken ct = default);

    /// <summary>
    /// Returns the top N fields with the most mismatches for a run.
    /// Only counts actual mismatches (both systems have the record but values differ),
    /// excluding _MISSING_ rows.
    /// </summary>
    Task<List<FieldMismatchStatDto>> GetTopMismatchFieldsAsync(
        int runId, int top = 10, CancellationToken ct = default);

    /// <summary>
    /// Returns the top N match keys (Werfcodes) with the most mismatches for a run.
    /// Only counts actual mismatches (both systems have the record but values differ),
    /// excluding _MISSING_ rows.
    /// </summary>
    Task<List<KeyMismatchStatDto>> GetTopMismatchKeysAsync(
        int runId, int top = 10, CancellationToken ct = default);
}