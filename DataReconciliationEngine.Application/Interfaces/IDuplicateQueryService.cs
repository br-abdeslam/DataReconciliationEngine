using DataReconciliationEngine.Application.DTOs;

namespace DataReconciliationEngine.Application.Interfaces;

public interface IDuplicateQueryService
{
    Task<List<DuplicateRunSummaryDto>> GetLastRunsAsync(int take = 10, CancellationToken ct = default);
    Task<DuplicateRunSummaryDto?> GetRunSummaryAsync(int runId, CancellationToken ct = default);

    Task<PagedResultDto<DuplicateGroupDto>> GetGroupsPageAsync(
        int runId,
        int page,
        int pageSize,
        string? sortLabel,
        bool sortDesc,
        int? minRecords,
        string? groupSearch,
        int? customerSitesId,
        CancellationToken ct = default);

    Task<List<DuplicateRecordDto>> GetRecordsForGroupAsync(int groupId, CancellationToken ct = default);
    Task<bool> DeleteRunAsync(int runId, CancellationToken ct = default);
}