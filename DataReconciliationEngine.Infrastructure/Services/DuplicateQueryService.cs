using System.Data;
using DataReconciliationEngine.Application.DTOs;
using DataReconciliationEngine.Application.Interfaces;
using DataReconciliationEngine.Domain.Entities;
using DataReconciliationEngine.Domain.Enums;
using DataReconciliationEngine.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace DataReconciliationEngine.Infrastructure.Services;

public sealed class DuplicateQueryService : IDuplicateQueryService
{
    private readonly ReconciliationDbContext _db;

    public DuplicateQueryService(ReconciliationDbContext db) => _db = db;

    public async Task<List<DuplicateRunSummaryDto>> GetLastRunsAsync(
        int take = 10, CancellationToken ct = default)
    {
        return await _db.ComparisonRuns
            .AsNoTracking()
            .Where(r => r.RunType == RunType.DuplicateCustomerSites)
            .OrderByDescending(r => r.RunDate)
            .Take(take)
            .Select(r => new DuplicateRunSummaryDto
            {
                RunId = r.Id,
                RunDate = r.RunDate,
                TotalRowsScanned = r.TotalRecords,
                DuplicateGroupsFound = r.TotalMismatches,
                TotalDuplicateRecords = r.TotalMissingInA
            })
            .ToListAsync(ct);
    }

    public async Task<DuplicateRunSummaryDto?> GetRunSummaryAsync(
        int runId, CancellationToken ct = default)
    {
        return await _db.ComparisonRuns
            .AsNoTracking()
            .Where(r => r.Id == runId && r.RunType == RunType.DuplicateCustomerSites)
            .Select(r => new DuplicateRunSummaryDto
            {
                RunId = r.Id,
                RunDate = r.RunDate,
                TotalRowsScanned = r.TotalRecords,
                DuplicateGroupsFound = r.TotalMismatches,
                TotalDuplicateRecords = r.TotalMissingInA
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResultDto<DuplicateGroupDto>> GetGroupsPageAsync(
        int runId, int page, int pageSize,
        string? sortLabel, bool sortDesc,
        int? minRecords, string? groupSearch, int? customerSitesId,
        CancellationToken ct = default)
    {
        var query = _db.DuplicateGroups
            .AsNoTracking()
            .Where(g => g.RunId == runId);

        if (minRecords is not null)
        {
            query = query.Where(g => g.RecordsCount >= minRecords.Value);
        }

        if (!string.IsNullOrWhiteSpace(groupSearch))
        {
            var search = groupSearch.Trim();
            if (Guid.TryParse(search, out var groupId))
            {
                query = query.Where(g => g.GroupId == groupId);
            }
            else
            {
                query = query.Where(g => g.CandidateKey.Contains(search));
            }
        }

        if (customerSitesId is not null)
        {
            var id = customerSitesId.Value;
            query = query.Where(g => g.Records.Any(r => r.CustomerSitesId == id));
        }

        var totalCount = await query.CountAsync(ct);

        query = sortLabel switch
        {
            "RecordsCount" => sortDesc
                ? query.OrderByDescending(g => g.RecordsCount)
                : query.OrderBy(g => g.RecordsCount),
            "GroupId" => sortDesc
                ? query.OrderByDescending(g => g.GroupId)
                : query.OrderBy(g => g.GroupId),
            _ => query.OrderByDescending(g => g.RecordsCount)
        };

        var items = await query
            .Skip(page * pageSize)
            .Take(pageSize)
            .Select(g => new DuplicateGroupDto
            {
                Id = g.Id,
                GroupId = g.GroupId,
                LatRound = g.LatRound,
                LonRound = g.LonRound,
                CandidateKey = g.CandidateKey,
                RecordsCount = g.RecordsCount,
                CreatedAt = g.CreatedAt
            })
            .ToListAsync(ct);

        return new PagedResultDto<DuplicateGroupDto>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    public async Task<List<DuplicateRecordDto>> GetRecordsForGroupAsync(
        int groupId, CancellationToken ct = default)
    {
        return await _db.DuplicateRecords
            .AsNoTracking()
            .Where(r => r.DuplicateGroupId == groupId)
            .OrderByDescending(r => r.CompletenessScore)
            .ThenBy(r => r.CustomerSitesId)
            .Select(r => new DuplicateRecordDto
            {
                Id = r.Id,
                CustomerSitesId = r.CustomerSitesId,
                StreetRaw = r.StreetRaw,
                NumberRaw = r.NumberRaw,
                BoxRaw = r.BoxRaw,
                ZipRaw = r.ZipRaw,
                CityRaw = r.CityRaw,
                StreetNorm = r.StreetNorm,
                NumberNorm = r.NumberNorm,
                BoxNorm = r.BoxNorm,
                ZipNorm = r.ZipNorm,
                CityNorm = r.CityNorm,
                Latitude = r.Latitude,
                Longitude = r.Longitude,
                CompletenessScore = r.CompletenessScore,
                IsMasterSuggested = r.IsMasterSuggested,
                Reason = r.Reason
            })
            .ToListAsync(ct);
    }

    public async Task<bool> DeleteRunAsync(int runId, CancellationToken ct = default)
    {
        var run = await _db.ComparisonRuns.FindAsync([runId], ct);
        if (run is null || run.RunType != RunType.DuplicateCustomerSites) return false;

        var conn = _db.Database.GetDbConnection();
        var wasOpen = conn.State == ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync(ct);

        try
        {
            await BatchDeleteAsync(conn, "DuplicateRecords",
                "DuplicateGroupId IN (SELECT Id FROM DuplicateGroups WHERE RunId = @runId)", runId, ct);
            await BatchDeleteAsync(conn, "DuplicateGroups", "RunId = @runId", runId, ct);

            _db.ComparisonRuns.Remove(run);
            await _db.SaveChangesAsync(ct);
            return true;
        }
        finally
        {
            if (!wasOpen) await conn.CloseAsync();
        }
    }

    private static async Task BatchDeleteAsync(
        System.Data.Common.DbConnection conn, string table, string where, int runId, CancellationToken ct)
    {
        int deleted;
        do
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"DELETE TOP (5000) FROM [{table}] WHERE {where}";
            var p = cmd.CreateParameter();
            p.ParameterName = "@runId";
            p.Value = runId;
            cmd.Parameters.Add(p);
            cmd.CommandTimeout = 120;
            deleted = await cmd.ExecuteNonQueryAsync(ct);
        } while (deleted == 5000);
    }
}