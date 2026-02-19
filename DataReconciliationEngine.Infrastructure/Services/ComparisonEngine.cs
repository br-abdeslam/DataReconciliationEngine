using System.Data;
using DataReconciliationEngine.Application.DTOs;
using DataReconciliationEngine.Application.Interfaces;
using DataReconciliationEngine.Domain.Entities;
using DataReconciliationEngine.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataReconciliationEngine.Infrastructure.Services;

/// <summary>
/// Loads a comparison configuration, queries both remote systems with dynamic SQL,
/// normalizes keys, detects missing/mismatched records, and persists results locally.
/// </summary>
public sealed class ComparisonEngine : IComparisonEngine
{
    private const int BatchSize = 2000;

    private readonly ReconciliationDbContext _localDb;
    private readonly SystemADbContext _systemADb;
    private readonly SystemBDbContext _systemBDb;
    private readonly ILogger<ComparisonEngine> _logger;

    public ComparisonEngine(
        ReconciliationDbContext localDb,
        SystemADbContext systemADb,
        SystemBDbContext systemBDb,
        ILogger<ComparisonEngine> logger)
    {
        _localDb = localDb;
        _systemADb = systemADb;
        _systemBDb = systemBDb;
        _logger = logger;
    }

    public async Task<RunComparisonSummary> RunComparisonAsync(
        RunComparisonRequest request,
        CancellationToken ct = default)
    {
        // ── 1. Load configuration + active mappings ────────────────
        var config = await _localDb.TableComparisonConfigurations
            .AsNoTracking()
            .Include(c => c.FieldMappings)
            .FirstOrDefaultAsync(c => c.Id == request.ComparisonConfigId, ct)
            ?? throw new InvalidOperationException(
                $"Comparison configuration {request.ComparisonConfigId} not found.");

        var activeMappings = config.FieldMappings
            .Where(m => m.IsActive)
            .ToList();

        if (activeMappings.Count == 0)
            throw new InvalidOperationException(
                $"Configuration '{config.ComparisonName}' has no active field mappings.");

        _logger.LogInformation(
            "Starting comparison '{Name}' (ConfigId={Id}) with {Count} field mapping(s).",
            config.ComparisonName, config.Id, activeMappings.Count);

        // ── 2. Read System A ───────────────────────────────────────
        var systemAData = await ReadSystemDataAsync(
            _systemADb,
            config.SystemA_Table,
            config.MatchColumn_SystemA,
            activeMappings.Select(m => m.SystemA_Column).ToList(),
            systemLabel: "A",
            ct);

        // ── 3. Read System B ───────────────────────────────────────
        var systemBData = await ReadSystemDataAsync(
            _systemBDb,
            config.SystemB_Table,
            config.MatchColumn_SystemB,
            activeMappings.Select(m => m.SystemB_Column).ToList(),
            systemLabel: "B",
            ct);

        // ── 4. Create ComparisonRun (persisted now to obtain Id) ───
        var run = new ComparisonRun
        {
            ComparisonConfigId = config.Id,
            RunDate = DateTime.UtcNow
        };
        _localDb.ComparisonRuns.Add(run);
        await _localDb.SaveChangesAsync(ct);

        _logger.LogInformation(
            "ComparisonRun {RunId} created. System A: {CountA} keys, System B: {CountB} keys.",
            run.Id, systemAData.Count, systemBData.Count);

        // ── 5. Compare ─────────────────────────────────────────────
        var keysA = systemAData.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var keysB = systemBData.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var batch = new List<ComparisonResult>(BatchSize);
        int totalMismatches = 0;
        int missingInA = 0;
        int missingInB = 0;

        // 5a. Keys in A but not in B → "missing in B"
        foreach (var key in keysA)
        {
            if (keysB.Contains(key)) continue;

            missingInB++;
            batch.Add(new ComparisonResult
            {
                RunId = run.Id,
                ComparisonConfigId = config.Id,
                MatchKeyValue = key,
                ExistsInSystemA = true,
                ExistsInSystemB = false,
                LogicalFieldName = "_MISSING_",
                IsMatch = false
            });

            if (batch.Count >= BatchSize)
                await FlushBatchAsync(batch, ct);
        }

        // 5b. Keys in B but not in A → "missing in A"
        foreach (var key in keysB)
        {
            if (keysA.Contains(key)) continue;

            missingInA++;
            batch.Add(new ComparisonResult
            {
                RunId = run.Id,
                ComparisonConfigId = config.Id,
                MatchKeyValue = key,
                ExistsInSystemA = false,
                ExistsInSystemB = true,
                LogicalFieldName = "_MISSING_",
                IsMatch = false
            });

            if (batch.Count >= BatchSize)
                await FlushBatchAsync(batch, ct);
        }

        // 5c. Matched keys → compare every active mapped field
        int matchedKeyCount = 0;

        foreach (var key in keysA)
        {
            if (!keysB.Contains(key)) continue;

            matchedKeyCount++;
            var rowA = systemAData[key];
            var rowB = systemBData[key];
            bool keyHasMismatch = false;

            foreach (var mapping in activeMappings)
            {
                var valA = rowA.GetValueOrDefault(mapping.SystemA_Column);
                var valB = rowB.GetValueOrDefault(mapping.SystemB_Column);

                var normalA = NormalizeValue(valA);
                var normalB = NormalizeValue(valB);

                bool isMatch = string.Equals(normalA, normalB, StringComparison.OrdinalIgnoreCase);
                if (isMatch) continue;

                // Store mismatch only
                keyHasMismatch = true;
                batch.Add(new ComparisonResult
                {
                    RunId = run.Id,
                    ComparisonConfigId = config.Id,
                    MatchKeyValue = key,
                    ExistsInSystemA = true,
                    ExistsInSystemB = true,
                    LogicalFieldName = mapping.LogicalFieldName,
                    ValueSystemA = valA,
                    ValueSystemB = valB,
                    IsMatch = false
                });

                if (batch.Count >= BatchSize)
                    await FlushBatchAsync(batch, ct);
            }

            if (keyHasMismatch) totalMismatches++;
        }

        // Final flush
        await FlushBatchAsync(batch, ct);

        // ── 6. Update run totals ───────────────────────────────────
        run.TotalRecords = matchedKeyCount;
        run.TotalMismatches = totalMismatches;
        run.TotalMissingInA = missingInA;
        run.TotalMissingInB = missingInB;
        await _localDb.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Comparison complete (RunId={RunId}). Matched={Matched}, Mismatches={Mis}, MissingInA={MisA}, MissingInB={MisB}.",
            run.Id, matchedKeyCount, totalMismatches, missingInA, missingInB);

        // ── 7. Return summary ──────────────────────────────────────
        return new RunComparisonSummary
        {
            RunId = run.Id,
            ComparisonConfigId = config.Id,
            TotalRecords = matchedKeyCount,
            TotalMismatches = totalMismatches,
            TotalMissingInA = missingInA,
            TotalMissingInB = missingInB
        };
    }

    // ═══════════════════════════════════════════════════════════════
    //  Private helpers
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Executes a dynamic SELECT on a remote system and returns rows keyed by the
    /// normalized match column value.  Only the columns listed in the active
    /// mappings are selected (no SELECT *).
    /// </summary>
    private async Task<Dictionary<string, Dictionary<string, string?>>> ReadSystemDataAsync(
        DbContext dbContext,
        string tableName,
        string matchColumn,
        List<string> dataColumns,
        string systemLabel,
        CancellationToken ct)
    {
        var result = new Dictionary<string, Dictionary<string, string?>>(
            StringComparer.OrdinalIgnoreCase);

        var connection = dbContext.Database.GetDbConnection();
        var wasOpen = connection.State == ConnectionState.Open;
        if (!wasOpen) await connection.OpenAsync(ct);

        try
        {
            using var cmd = connection.CreateCommand();

            // Build column list: [MatchCol], [Col1], [Col2], …
            var allColumns = new List<string> { matchColumn };
            foreach (var col in dataColumns)
            {
                if (!allColumns.Contains(col, StringComparer.OrdinalIgnoreCase))
                    allColumns.Add(col);
            }

            var columnsSql = string.Join(", ", allColumns.Select(QuoteName));

            // tableName is already fully qualified, e.g. [PRO_BE01].[dbo].[Company]
            cmd.CommandText = $"SELECT {columnsSql} FROM {tableName}";
            cmd.CommandTimeout = 120; // generous timeout for ~24 K rows

            _logger.LogDebug("System {Label} SQL: {Sql}", systemLabel, cmd.CommandText);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            int skippedNulls = 0;
            int skippedDuplicates = 0;

            while (await reader.ReadAsync(ct))
            {
                var rawKey = reader[matchColumn]?.ToString();
                if (string.IsNullOrWhiteSpace(rawKey))
                {
                    skippedNulls++;
                    continue;
                }

                var normalizedKey = rawKey.Trim().ToUpperInvariant();

                if (result.ContainsKey(normalizedKey))
                {
                    skippedDuplicates++;
                    continue; // keep the first occurrence
                }

                var row = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
                foreach (var col in dataColumns)
                {
                    var val = reader[col];
                    row[col] = val is DBNull ? null : val?.ToString();
                }

                result[normalizedKey] = row;
            }

            _logger.LogInformation(
                "System {Label}: loaded {Count} unique keys. Skipped {Nulls} null/empty, {Dups} duplicate(s).",
                systemLabel, result.Count, skippedNulls, skippedDuplicates);
        }
        finally
        {
            if (!wasOpen) await connection.CloseAsync();
        }

        return result;
    }

    /// <summary>Trims whitespace; collapses null/empty/whitespace to null for consistent comparison.</summary>
    private static string? NormalizeValue(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>Wraps a column name in square brackets, escaping any embedded <c>]</c>.</summary>
    private static string QuoteName(string identifier)
        => $"[{identifier.Replace("]", "]]")}]";

    /// <summary>
    /// Saves pending <see cref="ComparisonResult"/> entities to the local DB
    /// and detaches them from the change tracker to prevent memory bloat.
    /// </summary>
    private async Task FlushBatchAsync(List<ComparisonResult> batch, CancellationToken ct)
    {
        if (batch.Count == 0) return;

        _localDb.ComparisonResults.AddRange(batch);
        await _localDb.SaveChangesAsync(ct);

        // Detach only ComparisonResult entries — keep ComparisonRun tracked
        // so we can update its totals at the end.
        foreach (var entry in _localDb.ChangeTracker.Entries<ComparisonResult>().ToList())
            entry.State = EntityState.Detached;

        _logger.LogDebug("Flushed {Count} result(s) to local DB.", batch.Count);
        batch.Clear();
    }
}