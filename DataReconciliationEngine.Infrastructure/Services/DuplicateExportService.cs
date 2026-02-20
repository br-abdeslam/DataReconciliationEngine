using System.Text;
using DataReconciliationEngine.Application.DTOs;
using DataReconciliationEngine.Application.Interfaces;
using DataReconciliationEngine.Domain.Entities;
using DataReconciliationEngine.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace DataReconciliationEngine.Infrastructure.Services;

public sealed class DuplicateExportService : IDuplicateExportService
{
    private const int BatchSize = 5000;
    private readonly ReconciliationDbContext _db;

    public DuplicateExportService(ReconciliationDbContext db) => _db = db;

    public async Task<ExportFileDto> ExportGroupsCsvAsync(int runId, CancellationToken ct = default)
    {
        var sb = new StringBuilder(32 * 1024);
        sb.AppendLine("GroupId,LatRound,LonRound,CandidateKey,RecordsCount,MasterSuggestedSiteId");

        int skip = 0;
        int count;

        do
        {
            var batch = await _db.DuplicateGroups
                .AsNoTracking()
                .Where(g => g.RunId == runId)
                .OrderByDescending(g => g.RecordsCount)
                .Skip(skip).Take(BatchSize)
                .Select(g => new
                {
                    g.GroupId,
                    g.LatRound,
                    g.LonRound,
                    g.CandidateKey,
                    g.RecordsCount,
                    MasterSiteId = g.Records
                        .Where(r => r.IsMasterSuggested)
                        .Select(r => (int?)r.CustomerSitesId)
                        .FirstOrDefault()
                })
                .ToListAsync(ct);

            count = batch.Count;
            foreach (var g in batch)
            {
                sb.Append(g.GroupId).Append(',');
                sb.Append(g.LatRound).Append(',');
                sb.Append(g.LonRound).Append(',');
                sb.Append(Esc(g.CandidateKey)).Append(',');
                sb.Append(g.RecordsCount).Append(',');
                sb.AppendLine(g.MasterSiteId?.ToString() ?? "");
            }

            skip += BatchSize;
        } while (count == BatchSize);

        return Build($"DuplicateGroups_Run{runId}.csv", sb);
    }

    public async Task<ExportFileDto> ExportRecordsCsvAsync(int runId, CancellationToken ct = default)
    {
        var sb = new StringBuilder(64 * 1024);
        sb.AppendLine("GroupId,CandidateKey,CustomerSitesId,StreetRaw,NumberRaw,BoxRaw,ZipRaw,CityRaw,StreetNorm,NumberNorm,BoxNorm,ZipNorm,CityNorm,Latitude,Longitude,Score,IsMaster,Reason");

        int skip = 0;
        int count;

        do
        {
            var batch = await _db.DuplicateRecords
                .AsNoTracking()
                .Join(_db.DuplicateGroups.AsNoTracking().Where(g => g.RunId == runId),
                    r => r.DuplicateGroupId, g => g.Id,
                    (r, g) => new { r, g.GroupId, g.CandidateKey })
                .OrderBy(x => x.CandidateKey)
                .ThenByDescending(x => x.r.CompletenessScore)
                .Skip(skip).Take(BatchSize)
                .ToListAsync(ct);

            count = batch.Count;
            foreach (var x in batch)
            {
                sb.Append(x.GroupId).Append(',');
                sb.Append(Esc(x.CandidateKey)).Append(',');
                sb.Append(x.r.CustomerSitesId).Append(',');
                sb.Append(Esc(x.r.StreetRaw)).Append(',');
                sb.Append(Esc(x.r.NumberRaw)).Append(',');
                sb.Append(Esc(x.r.BoxRaw)).Append(',');
                sb.Append(Esc(x.r.ZipRaw)).Append(',');
                sb.Append(Esc(x.r.CityRaw)).Append(',');
                sb.Append(Esc(x.r.StreetNorm)).Append(',');
                sb.Append(Esc(x.r.NumberNorm)).Append(',');
                sb.Append(Esc(x.r.BoxNorm)).Append(',');
                sb.Append(Esc(x.r.ZipNorm)).Append(',');
                sb.Append(Esc(x.r.CityNorm)).Append(',');
                sb.Append(x.r.Latitude).Append(',');
                sb.Append(x.r.Longitude).Append(',');
                sb.Append(x.r.CompletenessScore).Append(',');
                sb.Append(x.r.IsMasterSuggested).Append(',');
                sb.AppendLine(Esc(x.r.Reason));
            }

            skip += BatchSize;
        } while (count == BatchSize);

        return Build($"DuplicateRecords_Run{runId}.csv", sb);
    }

    private static ExportFileDto Build(string fileName, StringBuilder sb) => new()
    {
        FileName = fileName,
        ContentType = "text/csv",
        Content = Encoding.UTF8.GetPreamble()
                    .Concat(Encoding.UTF8.GetBytes(sb.ToString()))
                    .ToArray()
    };

    private static string Esc(string? v)
    {
        if (string.IsNullOrEmpty(v)) return "";
        if (v.Contains(',') || v.Contains('"') || v.Contains('\n'))
            return $"\"{v.Replace("\"", "\"\"")}\"";
        return v;
    }
}