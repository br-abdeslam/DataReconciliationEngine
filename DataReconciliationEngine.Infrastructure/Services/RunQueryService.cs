using DataReconciliationEngine.Application.DTOs;
using DataReconciliationEngine.Application.Interfaces;
using DataReconciliationEngine.Domain.Entities;
using DataReconciliationEngine.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace DataReconciliationEngine.Infrastructure.Services;

/// <summary>
/// Read-only queries against the local ReconciliationDbContext.
/// All queries use AsNoTracking for performance.
/// </summary>
public sealed class RunQueryService : IRunQueryService
{
    private readonly ReconciliationDbContext _db;

    public RunQueryService(ReconciliationDbContext db) => _db = db;

    public async Task<List<ConfigSummaryDto>> GetActiveConfigurationsAsync(CancellationToken ct = default)
    {
        return await _db.TableComparisonConfigurations
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.ComparisonName)
            .Select(c => new ConfigSummaryDto
            {
                Id = c.Id,
                ComparisonName = c.ComparisonName,
                IsActive = c.IsActive
            })
            .ToListAsync(ct);
    }

    public async Task<List<RunSummaryDto>> GetLastRunsAsync(int configId, int take = 10, CancellationToken ct = default)
    {
        return await _db.ComparisonRuns
            .AsNoTracking()
            .Where(r => r.ComparisonConfigId == configId)
            .OrderByDescending(r => r.RunDate)
            .Take(take)
            .Join(
                _db.TableComparisonConfigurations,
                r => r.ComparisonConfigId!.Value,   // int? → int (safe: filtered above)
                c => c.Id,
                (r, c) => new RunSummaryDto
                {
                    RunId = r.Id,
                    ComparisonConfigId = r.ComparisonConfigId!.Value,
                    ComparisonName = c.ComparisonName,
                    RunDate = r.RunDate,
                    TotalRecords = r.TotalRecords,
                    TotalMismatches = r.TotalMismatches,
                    TotalMissingInA = r.TotalMissingInA,
                    TotalMissingInB = r.TotalMissingInB
                })
            .ToListAsync(ct);
    }

    public async Task<RunSummaryDto?> GetRunSummaryAsync(int runId, CancellationToken ct = default)
    {
        return await _db.ComparisonRuns
            .AsNoTracking()
            .Where(r => r.Id == runId)
            .Join(
                _db.TableComparisonConfigurations,
                r => r.ComparisonConfigId!.Value,   // int? → int (comparison runs always have config)
                c => c.Id,
                (r, c) => new RunSummaryDto
                {
                    RunId = r.Id,
                    ComparisonConfigId = r.ComparisonConfigId!.Value,
                    ComparisonName = c.ComparisonName,
                    RunDate = r.RunDate,
                    TotalRecords = r.TotalRecords,
                    TotalMismatches = r.TotalMismatches,
                    TotalMissingInA = r.TotalMissingInA,
                    TotalMissingInB = r.TotalMissingInB
                })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResultDto<ResultPageDto>> GetResultsPageAsync(ResultFilterDto filter, CancellationToken ct = default)
    {
        IQueryable<ComparisonResult> query = _db.ComparisonResults
            .AsNoTracking()
            .Where(r => r.RunId == filter.RunId);

        // ── Filters ────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(filter.SearchKey))
        {
            var search = filter.SearchKey.Trim();
            query = query.Where(r => r.MatchKeyValue.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(filter.FieldName))
        {
            query = query.Where(r => r.LogicalFieldName == filter.FieldName);
        }

        if (filter.OnlyMissing)
        {
            query = query.Where(r => r.LogicalFieldName == "_MISSING_");
        }
        else if (filter.OnlyMismatches)
        {
            query = query.Where(r => !r.IsMatch);
        }

        // ── Count (before paging) ──────────────────────────────
        var totalCount = await query.CountAsync(ct);

        // ── Sorting ────────────────────────────────────────────
        query = filter.SortField switch
        {
            nameof(ComparisonResult.LogicalFieldName) => filter.SortDescending
                ? query.OrderByDescending(r => r.LogicalFieldName)
                : query.OrderBy(r => r.LogicalFieldName),

            nameof(ComparisonResult.IsMatch) => filter.SortDescending
                ? query.OrderByDescending(r => r.IsMatch)
                : query.OrderBy(r => r.IsMatch),

            // Default: sort by MatchKeyValue
            _ => filter.SortDescending
                ? query.OrderByDescending(r => r.MatchKeyValue)
                : query.OrderBy(r => r.MatchKeyValue)
        };

        // ── Paging ─────────────────────────────────────────────
        var items = await query
            .Skip(filter.Page * filter.PageSize)
            .Take(filter.PageSize)
            .Select(r => new ResultPageDto
            {
                Id = r.Id,
                MatchKeyValue = r.MatchKeyValue,
                LogicalFieldName = r.LogicalFieldName,
                ValueSystemA = r.ValueSystemA,
                ValueSystemB = r.ValueSystemB,
                ExistsInSystemA = r.ExistsInSystemA,
                ExistsInSystemB = r.ExistsInSystemB,
                IsMatch = r.IsMatch,
                ComparedAt = r.ComparedAt
            })
            .ToListAsync(ct);

        return new PagedResultDto<ResultPageDto>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    public async Task<List<ResultPageDto>> GetResultsByKeyAsync(int runId, string matchKeyValue, CancellationToken ct = default)
    {
        return await _db.ComparisonResults
            .AsNoTracking()
            .Where(r => r.RunId == runId && r.MatchKeyValue == matchKeyValue)
            .OrderBy(r => r.LogicalFieldName)
            .Select(r => new ResultPageDto
            {
                Id = r.Id,
                MatchKeyValue = r.MatchKeyValue,
                LogicalFieldName = r.LogicalFieldName,
                ValueSystemA = r.ValueSystemA,
                ValueSystemB = r.ValueSystemB,
                ExistsInSystemA = r.ExistsInSystemA,
                ExistsInSystemB = r.ExistsInSystemB,
                IsMatch = r.IsMatch,
                ComparedAt = r.ComparedAt
            })
            .ToListAsync(ct);
    }

    public async Task<List<string>> GetFieldNamesForRunAsync(int runId, CancellationToken ct = default)
    {
        return await _db.ComparisonResults
            .AsNoTracking()
            .Where(r => r.RunId == runId)
            .Select(r => r.LogicalFieldName)
            .Distinct()
            .OrderBy(f => f)
            .ToListAsync(ct);
    }

    public async Task<bool> DeleteRunAsync(int runId, CancellationToken ct = default)
    {
        var run = await _db.ComparisonRuns.FindAsync([runId], ct);
        if (run is null) return false;

        // Use raw ADO.NET with extended timeout + batched deletes
        // LocalDB is slow — delete in chunks to avoid timeout
        var conn = _db.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync(ct);

        try
        {
            // Delete in batches of 5000 to keep transaction log small
            int deleted;
            do
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = """
                    DELETE TOP (5000) FROM ComparisonResults
                    WHERE RunId = @runId
                    """;
                var param = cmd.CreateParameter();
                param.ParameterName = "@runId";
                param.Value = runId;
                cmd.Parameters.Add(param);
                cmd.CommandTimeout = 120;

                deleted = await cmd.ExecuteNonQueryAsync(ct);
            } while (deleted == 5000);

            // Now delete the run itself
            _db.ComparisonRuns.Remove(run);
            await _db.SaveChangesAsync(ct);
            return true;
        }
        finally
        {
            if (!wasOpen) await conn.CloseAsync();
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  Analytics
    // ═══════════════════════════════════════════════════════════

    public async Task<List<FieldMismatchStatDto>> GetTopMismatchFieldsAsync(
        int runId, int top = 10, CancellationToken ct = default)
    {
        // Only actual mismatches: both systems have the record but values differ.
        // Excludes _MISSING_ rows (where one system doesn't have the key at all).
        return await _db.ComparisonResults
            .AsNoTracking()
            .Where(r => r.RunId == runId
                     && !r.IsMatch
                     && r.ExistsInSystemA
                     && r.ExistsInSystemB)
            .GroupBy(r => r.LogicalFieldName)
            .Select(g => new FieldMismatchStatDto
            {
                LogicalFieldName = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToListAsync(ct);
    }

    public async Task<List<KeyMismatchStatDto>> GetTopMismatchKeysAsync(
        int runId, int top = 10, CancellationToken ct = default)
    {
        // Counts all non-match rows per key (mismatches + missing).
        // This gives the "most problematic sites" — keys with the most issues overall.
        return await _db.ComparisonResults
            .AsNoTracking()
            .Where(r => r.RunId == runId && !r.IsMatch)
            .GroupBy(r => r.MatchKeyValue)
            .Select(g => new KeyMismatchStatDto
            {
                MatchKeyValue = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToListAsync(ct);
    }
}