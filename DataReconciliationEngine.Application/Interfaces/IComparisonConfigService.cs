using DataReconciliationEngine.Application.DTOs;

namespace DataReconciliationEngine.Application.Interfaces;

/// <summary>
/// CRUD service for comparison configurations.
/// </summary>
public interface IComparisonConfigService
{
    /// <summary>Returns all configurations with mapping/run counts.</summary>
    Task<List<ComparisonConfigDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Returns a single configuration for editing.</summary>
    Task<ComparisonConfigDto?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Creates a new configuration. Returns the created DTO or an error.</summary>
    Task<ServiceResult<ComparisonConfigDto>> CreateAsync(ComparisonConfigEditDto dto, CancellationToken ct = default);

    /// <summary>Updates an existing configuration. Returns the updated DTO or an error.</summary>
    Task<ServiceResult<ComparisonConfigDto>> UpdateAsync(int id, ComparisonConfigEditDto dto, CancellationToken ct = default);

    /// <summary>Deletes a configuration. Fails if it has runs (FK restrict).</summary>
    Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Toggles the IsActive flag.</summary>
    Task<ServiceResult> ToggleActiveAsync(int id, bool isActive, CancellationToken ct = default);
}