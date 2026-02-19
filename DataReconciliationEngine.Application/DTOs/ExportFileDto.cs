namespace DataReconciliationEngine.Application.DTOs;

/// <summary>
/// Result of an export operation — carries the file content for download.
/// </summary>
public sealed class ExportFileDto
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required byte[] Content { get; init; }
}