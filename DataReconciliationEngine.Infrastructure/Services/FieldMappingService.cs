using DataReconciliationEngine.Application.DTOs;
using DataReconciliationEngine.Application.Interfaces;
using DataReconciliationEngine.Domain.Entities;
using DataReconciliationEngine.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace DataReconciliationEngine.Infrastructure.Services;

public sealed class FieldMappingService : IFieldMappingService
{
    private readonly ReconciliationDbContext _db;

    public FieldMappingService(ReconciliationDbContext db) => _db = db;

    public async Task<List<FieldMappingDto>> GetByConfigIdAsync(int configId, CancellationToken ct = default)
    {
        return await _db.FieldMappingConfigurations
            .AsNoTracking()
            .Where(m => m.ComparisonConfigId == configId)
            .OrderBy(m => m.LogicalFieldName)
            .Select(m => new FieldMappingDto
            {
                Id = m.Id,
                ComparisonConfigId = m.ComparisonConfigId,
                LogicalFieldName = m.LogicalFieldName,
                SystemA_Column = m.SystemA_Column,
                SystemB_Column = m.SystemB_Column,
                IsActive = m.IsActive
            })
            .ToListAsync(ct);
    }

    public async Task<FieldMappingDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.FieldMappingConfigurations
            .AsNoTracking()
            .Where(m => m.Id == id)
            .Select(m => new FieldMappingDto
            {
                Id = m.Id,
                ComparisonConfigId = m.ComparisonConfigId,
                LogicalFieldName = m.LogicalFieldName,
                SystemA_Column = m.SystemA_Column,
                SystemB_Column = m.SystemB_Column,
                IsActive = m.IsActive
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ServiceResult<FieldMappingDto>> CreateAsync(
        int configId, FieldMappingEditDto dto, CancellationToken ct = default)
    {
        // Verify the parent config exists
        var configExists = await _db.TableComparisonConfigurations
            .AnyAsync(c => c.Id == configId, ct);

        if (!configExists)
            return ServiceResult<FieldMappingDto>.Failure("Configuration not found.");

        // Check for duplicate logical field name within the same config
        var duplicate = await _db.FieldMappingConfigurations
            .AnyAsync(m => m.ComparisonConfigId == configId
                        && m.LogicalFieldName == dto.LogicalFieldName.Trim(), ct);

        if (duplicate)
            return ServiceResult<FieldMappingDto>.Failure(
                $"A mapping named \"{dto.LogicalFieldName}\" already exists in this configuration.");

        var entity = new FieldMappingConfiguration
        {
            ComparisonConfigId = configId,
            LogicalFieldName = dto.LogicalFieldName.Trim(),
            SystemA_Column = dto.SystemA_Column.Trim(),
            SystemB_Column = dto.SystemB_Column.Trim(),
            IsActive = dto.IsActive
        };

        _db.FieldMappingConfigurations.Add(entity);
        await _db.SaveChangesAsync(ct);

        var result = await GetByIdAsync(entity.Id, ct);
        return ServiceResult<FieldMappingDto>.Success(result!);
    }

    public async Task<ServiceResult<FieldMappingDto>> UpdateAsync(
        int id, FieldMappingEditDto dto, CancellationToken ct = default)
    {
        var entity = await _db.FieldMappingConfigurations.FindAsync([id], ct);
        if (entity is null)
            return ServiceResult<FieldMappingDto>.Failure("Mapping not found.");

        // Check for duplicate logical field name (excluding self)
        var duplicate = await _db.FieldMappingConfigurations
            .AnyAsync(m => m.ComparisonConfigId == entity.ComparisonConfigId
                        && m.LogicalFieldName == dto.LogicalFieldName.Trim()
                        && m.Id != id, ct);

        if (duplicate)
            return ServiceResult<FieldMappingDto>.Failure(
                $"A mapping named \"{dto.LogicalFieldName}\" already exists in this configuration.");

        entity.LogicalFieldName = dto.LogicalFieldName.Trim();
        entity.SystemA_Column = dto.SystemA_Column.Trim();
        entity.SystemB_Column = dto.SystemB_Column.Trim();
        entity.IsActive = dto.IsActive;

        await _db.SaveChangesAsync(ct);

        var result = await GetByIdAsync(entity.Id, ct);
        return ServiceResult<FieldMappingDto>.Success(result!);
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.FieldMappingConfigurations.FindAsync([id], ct);
        if (entity is null)
            return ServiceResult.Failure("Mapping not found.");

        _db.FieldMappingConfigurations.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> ToggleActiveAsync(int id, bool isActive, CancellationToken ct = default)
    {
        var entity = await _db.FieldMappingConfigurations.FindAsync([id], ct);
        if (entity is null)
            return ServiceResult.Failure("Mapping not found.");

        entity.IsActive = isActive;
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }
}