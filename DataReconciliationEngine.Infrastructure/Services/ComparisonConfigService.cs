using DataReconciliationEngine.Application.DTOs;
using DataReconciliationEngine.Application.Interfaces;
using DataReconciliationEngine.Domain.Entities;
using DataReconciliationEngine.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace DataReconciliationEngine.Infrastructure.Services;

public sealed class ComparisonConfigService : IComparisonConfigService
{
    private readonly ReconciliationDbContext _db;

    public ComparisonConfigService(ReconciliationDbContext db) => _db = db;

    public async Task<List<ComparisonConfigDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.TableComparisonConfigurations
            .AsNoTracking()
            .OrderBy(c => c.ComparisonName)
            .Select(c => new ComparisonConfigDto
            {
                Id = c.Id,
                ComparisonName = c.ComparisonName,
                SystemA_Table = c.SystemA_Table,
                SystemB_Table = c.SystemB_Table,
                MatchColumn_SystemA = c.MatchColumn_SystemA,
                MatchColumn_SystemB = c.MatchColumn_SystemB,
                IsActive = c.IsActive,
                FieldMappingCount = c.FieldMappings.Count,
                RunCount = _db.ComparisonRuns.Count(r => r.ComparisonConfigId == c.Id)
            })
            .ToListAsync(ct);
    }

    public async Task<ComparisonConfigDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.TableComparisonConfigurations
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new ComparisonConfigDto
            {
                Id = c.Id,
                ComparisonName = c.ComparisonName,
                SystemA_Table = c.SystemA_Table,
                SystemB_Table = c.SystemB_Table,
                MatchColumn_SystemA = c.MatchColumn_SystemA,
                MatchColumn_SystemB = c.MatchColumn_SystemB,
                IsActive = c.IsActive,
                FieldMappingCount = c.FieldMappings.Count,
                RunCount = _db.ComparisonRuns.Count(r => r.ComparisonConfigId == c.Id)
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ServiceResult<ComparisonConfigDto>> CreateAsync(
        ComparisonConfigEditDto dto, CancellationToken ct = default)
    {
        var entity = new TableComparisonConfiguration
        {
            ComparisonName = dto.ComparisonName.Trim(),
            SystemA_Table = dto.SystemA_Table.Trim(),
            SystemB_Table = dto.SystemB_Table.Trim(),
            MatchColumn_SystemA = dto.MatchColumn_SystemA.Trim(),
            MatchColumn_SystemB = dto.MatchColumn_SystemB.Trim(),
            IsActive = dto.IsActive
        };

        _db.TableComparisonConfigurations.Add(entity);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return ServiceResult<ComparisonConfigDto>.Failure(
                $"A configuration named \"{dto.ComparisonName}\" already exists.");
        }

        var result = await GetByIdAsync(entity.Id, ct);
        return ServiceResult<ComparisonConfigDto>.Success(result!);
    }

    public async Task<ServiceResult<ComparisonConfigDto>> UpdateAsync(
        int id, ComparisonConfigEditDto dto, CancellationToken ct = default)
    {
        var entity = await _db.TableComparisonConfigurations.FindAsync([id], ct);
        if (entity is null)
            return ServiceResult<ComparisonConfigDto>.Failure("Configuration not found.");

        entity.ComparisonName = dto.ComparisonName.Trim();
        entity.SystemA_Table = dto.SystemA_Table.Trim();
        entity.SystemB_Table = dto.SystemB_Table.Trim();
        entity.MatchColumn_SystemA = dto.MatchColumn_SystemA.Trim();
        entity.MatchColumn_SystemB = dto.MatchColumn_SystemB.Trim();
        entity.IsActive = dto.IsActive;

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return ServiceResult<ComparisonConfigDto>.Failure(
                $"A configuration named \"{dto.ComparisonName}\" already exists.");
        }

        var result = await GetByIdAsync(entity.Id, ct);
        return ServiceResult<ComparisonConfigDto>.Success(result!);
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.TableComparisonConfigurations.FindAsync([id], ct);
        if (entity is null)
            return ServiceResult.Failure("Configuration not found.");

        // Check for existing runs (FK Restrict prevents delete)
        var hasRuns = await _db.ComparisonRuns
            .AnyAsync(r => r.ComparisonConfigId == id, ct);

        if (hasRuns)
            return ServiceResult.Failure(
                "Cannot delete this configuration because it has comparison runs. " +
                "Deactivate it instead.");

        _db.TableComparisonConfigurations.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> ToggleActiveAsync(int id, bool isActive, CancellationToken ct = default)
    {
        var entity = await _db.TableComparisonConfigurations.FindAsync([id], ct);
        if (entity is null)
            return ServiceResult.Failure("Configuration not found.");

        entity.IsActive = isActive;
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    /// <summary>
    /// Detects SQL Server unique constraint violation (error 2601 / 2627).
    /// </summary>
    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        return ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx
            && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
    }
}