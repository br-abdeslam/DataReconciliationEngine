using DataReconciliationEngine.Application.DTOs;

namespace DataReconciliationEngine.Application.Interfaces;

/// <summary>
/// Service for exporting comparison results to file formats (CSV).
/// </summary>
public interface IResultExportService
{
    /// <summary>
    /// Generates a CSV file from comparison results, applying the given filters.
    /// Returns a DTO containing the file name, content type, and byte content.
    /// </summary>
    Task<ExportFileDto> ExportCsvAsync(ExportRequestDto request, CancellationToken ct = default);
}