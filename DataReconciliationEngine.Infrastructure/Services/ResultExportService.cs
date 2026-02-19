using System.Text;
using DataReconciliationEngine.Application.DTOs;
using DataReconciliationEngine.Application.Interfaces;
using DataReconciliationEngine.Domain.Entities;
using DataReconciliationEngine.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace DataReconciliationEngine.Infrastructure.Services;

public sealed class ResultExportService : IResultExportService
{
    private const int BatchSize = 5000;

    private readonly ReconciliationDbContext _db;

    public ResultExportService(ReconciliationDbContext db) => _db = db;

    public async Task<ExportFileDto> ExportCsvAsync(
        ExportRequestDto request, CancellationToken ct = default)
    {
        var query = BuildFilteredQuery(request);

        // Order deterministically for consistent exports
        query = query.OrderBy(r => r.MatchKeyValue)
                     .ThenBy(r => r.LogicalFieldName);

        var sb = new StringBuilder(64 * 1024); // pre-allocate ~64 KB

        // ── Header ─────────────────────────────────────────────
        sb.AppendLine("RunId,MatchKeyValue,LogicalFieldName,ExistsInSystemA,ExistsInSystemB,ValueSystemA,ValueSystemB,IsMatch,ComparedAt");

        // ── Batched reads ──────────────────────────────────────
        int skip = 0;
        int rowCount;

        do
        {
            var batch = await query
                .Skip(skip)
                .Take(BatchSize)
                .Select(r => new
                {
                    r.RunId,
                    r.MatchKeyValue,
                    r.LogicalFieldName,
                    r.ExistsInSystemA,
                    r.ExistsInSystemB,
                    r.ValueSystemA,
                    r.ValueSystemB,
                    r.IsMatch,
                    r.ComparedAt
                })
                .ToListAsync(ct);

            rowCount = batch.Count;

            foreach (var r in batch)
            {
                sb.Append(r.RunId).Append(',');
                sb.Append(CsvEscape(r.MatchKeyValue)).Append(',');
                sb.Append(CsvEscape(r.LogicalFieldName)).Append(',');
                sb.Append(r.ExistsInSystemA).Append(',');
                sb.Append(r.ExistsInSystemB).Append(',');
                sb.Append(CsvEscape(r.ValueSystemA)).Append(',');
                sb.Append(CsvEscape(r.ValueSystemB)).Append(',');
                sb.Append(r.IsMatch).Append(',');
                sb.AppendLine(r.ComparedAt.ToString("o")); // ISO 8601
            }

            skip += BatchSize;

        } while (rowCount == BatchSize);

        // ── Build result ───────────────────────────────────────
        var fileName = $"Run_{request.RunId}_{request.FileLabel}.csv";

        return new ExportFileDto
        {
            FileName = fileName,
            ContentType = "text/csv",
            Content = Encoding.UTF8.GetPreamble()
                        .Concat(Encoding.UTF8.GetBytes(sb.ToString()))
                        .ToArray()
        };
    }

    // ═══════════════════════════════════════════════════════════
    //  Private helpers
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Builds a filtered IQueryable matching the same logic as
    /// <see cref="RunQueryService.GetResultsPageAsync"/>.
    /// </summary>
    private IQueryable<ComparisonResult> BuildFilteredQuery(ExportRequestDto request)
    {
        IQueryable<ComparisonResult> query = _db.ComparisonResults
            .AsNoTracking()
            .Where(r => r.RunId == request.RunId);

        if (!string.IsNullOrWhiteSpace(request.SearchKey))
        {
            var search = request.SearchKey.Trim();
            query = query.Where(r => r.MatchKeyValue.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.FieldName))
        {
            query = query.Where(r => r.LogicalFieldName == request.FieldName);
        }

        if (request.OnlyMissing)
        {
            query = query.Where(r => r.LogicalFieldName == "_MISSING_");
        }
        else if (request.OnlyMismatches)
        {
            query = query.Where(r => !r.IsMatch);
        }

        return query;
    }

    /// <summary>
    /// Escapes a value for RFC 4180 CSV:
    /// - Wraps in double quotes if it contains comma, quote, or newline.
    /// - Doubles any embedded double quotes.
    /// - Null → empty string.
    /// </summary>
    private static string CsvEscape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }
}