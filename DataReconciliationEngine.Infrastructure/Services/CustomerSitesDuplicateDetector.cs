using System.Data;
using System.Data.Common;
using DataReconciliationEngine.Application.DTOs;
using DataReconciliationEngine.Application.Interfaces;
using DataReconciliationEngine.Domain.Entities;
using DataReconciliationEngine.Domain.Enums;
using DataReconciliationEngine.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataReconciliationEngine.Infrastructure.Services;

/// <summary>
/// Queries Company.dbo.customer_sites (read-only), detects duplicate clusters,
/// normalizes addresses, scores records, and persists findings to LocalDB.
/// </summary>
public sealed class CustomerSitesDuplicateDetector : ICustomerSitesDuplicateDetector
{
    private const int FlushThreshold = 2000; // flush every N records

    private readonly ReconciliationDbContext _localDb;
    private readonly SystemBDbContext _sourceDb;
    private readonly ILogger<CustomerSitesDuplicateDetector> _logger;

    public CustomerSitesDuplicateDetector(
        ReconciliationDbContext localDb,
        SystemBDbContext sourceDb,
        ILogger<CustomerSitesDuplicateDetector> logger)
    {
        _localDb = localDb;
        _sourceDb = sourceDb;
        _logger = logger;
    }

    public async Task<DuplicateDetectionSummary> RunDetectionAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting duplicate customer-sites detection…");

        // ── 1. Create run ──────────────────────────────────────
        var run = new ComparisonRun
        {
            RunType = RunType.DuplicateCustomerSites,
            ComparisonConfigId = null,
            RunDate = DateTime.UtcNow
        };
        _localDb.ComparisonRuns.Add(run);
        await _localDb.SaveChangesAsync(ct);

        _logger.LogInformation("Created duplicate-detection run {RunId}.", run.Id);

        // ── 2. Stream duplicate rows from source DB ────────────
        var conn = _sourceDb.Database.GetDbConnection();
        var wasOpen = conn.State == ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync(ct);

        int totalRowsScanned = 0;
        int totalGroups = 0;
        int totalRecords = 0;
        int pendingRecords = 0;

        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = DuplicateRowsSql;
            cmd.CommandTimeout = 300;

            using var reader = await cmd.ExecuteReaderAsync(ct);

            string currentKey = "";
            var currentGroupRows = new List<SourceRow>();

            while (await reader.ReadAsync(ct))
            {
                totalRowsScanned++;
                var row = MapRow(reader);
                var key = BuildCandidateKey(row.GroupId, row.LatRound, row.LonRound);

                // Key changed → process the completed group
                if (key != currentKey && currentGroupRows.Count > 0)
                {
                    var group = BuildGroup(run.Id, currentKey, currentGroupRows);
                    _localDb.DuplicateGroups.Add(group);
                    totalGroups++;
                    totalRecords += group.RecordsCount;
                    pendingRecords += group.RecordsCount;

                    if (pendingRecords >= FlushThreshold)
                    {
                        await FlushAsync(ct);
                        pendingRecords = 0;
                    }

                    currentGroupRows.Clear();
                }

                currentKey = key;
                currentGroupRows.Add(row);
            }

            // Process last group
            if (currentGroupRows.Count > 0)
            {
                var group = BuildGroup(run.Id, currentKey, currentGroupRows);
                _localDb.DuplicateGroups.Add(group);
                totalGroups++;
                totalRecords += group.RecordsCount;
            }

            // Final flush
            await FlushAsync(ct);
        }
        finally
        {
            if (!wasOpen) await conn.CloseAsync();
        }

        // ── 3. Update run totals ───────────────────────────────
        run.TotalRecords = totalRowsScanned;
        run.TotalMismatches = totalGroups;       // reuse: "groups found"
        run.TotalMissingInA = totalRecords;      // reuse: "total duplicate records"
        run.TotalMissingInB = 0;
        await _localDb.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Duplicate detection complete (RunId={RunId}). Rows={Rows}, Groups={Groups}, Records={Records}.",
            run.Id, totalRowsScanned, totalGroups, totalRecords);

        return new DuplicateDetectionSummary
        {
            RunId = run.Id,
            TotalRowsScanned = totalRowsScanned,
            DuplicateGroupsFound = totalGroups,
            TotalDuplicateRecords = totalRecords
        };
    }

    // ═════════════════════════════════════════════════════════
    //  SQL — single query returns all rows in duplicate groups
    // ═════════════════════════════════════════════════════════

    private const string DuplicateRowsSql = """
        SELECT
            cs.Customer_Sites_ID,
            cs.Group_ID,
            cs.Street,
            cs.Number,
            cs.Box,
            cs.Zipcode,
            cs.City,
            cs.Latitude,
            cs.Longitude,
            cs.Created_at,
            ISNULL(cs.IsVerified, 0) AS IsVerified,
            ISNULL(cs.FromKBO, 0)    AS FromKBO
        FROM dbo.customer_sites cs
        INNER JOIN (
            SELECT
                Group_ID,
                ROUND(Latitude, 6) AS LatRound,
                ROUND(Longitude, 6) AS LonRound
            FROM dbo.customer_sites
            WHERE Group_ID IS NOT NULL
              AND Latitude  IS NOT NULL
              AND Longitude IS NOT NULL
            GROUP BY Group_ID, ROUND(Latitude, 6), ROUND(Longitude, 6)
            HAVING COUNT(*) > 1
        ) dup
            ON  cs.Group_ID = dup.Group_ID
            AND ROUND(cs.Latitude, 6)  = dup.LatRound
            AND ROUND(cs.Longitude, 6) = dup.LonRound
        ORDER BY
            cs.Group_ID,
            ROUND(cs.Latitude, 6),
            ROUND(cs.Longitude, 6),
            cs.Customer_Sites_ID
        """;

    // ═════════════════════════════════════════════════════════
    //  Row mapping + group building
    // ═════════════════════════════════════════════════════════

    private sealed record SourceRow(
        int CustomerSitesId,
        Guid GroupId,
        string? Street,
        string? Number,
        string? Box,
        string? Zipcode,
        string? City,
        decimal Latitude,
        decimal Longitude,
        decimal LatRound,
        decimal LonRound,
        DateTime? CreatedAt,
        bool IsVerified,
        bool FromKBO);

    private static SourceRow MapRow(DbDataReader r)
    {
        var lat = r.GetDecimal(r.GetOrdinal("Latitude"));
        var lon = r.GetDecimal(r.GetOrdinal("Longitude"));

        return new SourceRow(
            CustomerSitesId: r.GetInt32(r.GetOrdinal("Customer_Sites_ID")),
            GroupId: r.GetGuid(r.GetOrdinal("Group_ID")),
            Street: r["Street"] as string,
            Number: r["Number"] as string,
            Box: r["Box"] as string,
            Zipcode: r["Zipcode"] as string,
            City: r["City"] as string,
            Latitude: lat,
            Longitude: lon,
            LatRound: Math.Round(lat, 6),
            LonRound: Math.Round(lon, 6),
            CreatedAt: r["Created_at"] as DateTime?,
            IsVerified: r.GetBoolean(r.GetOrdinal("IsVerified")),
            FromKBO: r.GetBoolean(r.GetOrdinal("FromKBO")));
    }

    private static string BuildCandidateKey(Guid groupId, decimal latRound, decimal lonRound)
        => $"{groupId}|{latRound}|{lonRound}";

    private static DuplicateGroup BuildGroup(int runId, string candidateKey, List<SourceRow> rows)
    {
        var records = new List<DuplicateRecord>(rows.Count);

        foreach (var row in rows)
        {
            var (numberNorm, boxNorm) = AddressNormalizer.ParseNumberBox(row.Number, row.Box);
            var streetNorm = AddressNormalizer.NormalizeText(row.Street);
            var zipNorm = AddressNormalizer.NormalizeText(row.Zipcode);
            var cityNorm = AddressNormalizer.NormalizeText(row.City);

            var score = AddressNormalizer.ComputeCompletenessScore(
                streetNorm, zipNorm, cityNorm, numberNorm, boxNorm,
                row.IsVerified, row.FromKBO);

            records.Add(new DuplicateRecord
            {
                CustomerSitesId = row.CustomerSitesId,
                StreetRaw = row.Street,
                NumberRaw = row.Number,
                BoxRaw = row.Box,
                ZipRaw = row.Zipcode,
                CityRaw = row.City,
                StreetNorm = streetNorm,
                NumberNorm = numberNorm,
                BoxNorm = boxNorm,
                ZipNorm = zipNorm,
                CityNorm = cityNorm,
                Latitude = row.Latitude,
                Longitude = row.Longitude,
                CompletenessScore = score,
                IsMasterSuggested = false
            });
        }

        // Mark master: highest score → oldest Created_at as tie-breaker
        var best = records
            .Select((rec, idx) => (Record: rec, Source: rows[idx]))
            .OrderByDescending(x => x.Record.CompletenessScore)
            .ThenBy(x => x.Source.CreatedAt ?? DateTime.MaxValue)
            .First();

        best.Record.IsMasterSuggested = true;
        best.Record.Reason = $"Highest score ({best.Record.CompletenessScore}) + oldest";

        var first = rows[0];
        return new DuplicateGroup
        {
            RunId = runId,
            GroupId = first.GroupId,
            LatRound = first.LatRound,
            LonRound = first.LonRound,
            CandidateKey = candidateKey,
            RecordsCount = records.Count,
            Records = records
        };
    }

    private async Task FlushAsync(CancellationToken ct)
    {
        await _localDb.SaveChangesAsync(ct);

        // Detach flushed entities to keep memory flat
        foreach (var e in _localDb.ChangeTracker.Entries<DuplicateGroup>().ToList())
            e.State = EntityState.Detached;
        foreach (var e in _localDb.ChangeTracker.Entries<DuplicateRecord>().ToList())
            e.State = EntityState.Detached;

        _logger.LogDebug("Flushed duplicate-detection batch to local DB.");
    }
}