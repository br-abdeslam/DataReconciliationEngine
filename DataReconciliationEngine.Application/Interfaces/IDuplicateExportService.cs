using DataReconciliationEngine.Application.DTOs;

namespace DataReconciliationEngine.Application.Interfaces;

public interface IDuplicateExportService
{
    Task<ExportFileDto> ExportGroupsCsvAsync(int runId, CancellationToken ct = default);
    Task<ExportFileDto> ExportRecordsCsvAsync(int runId, CancellationToken ct = default);
}