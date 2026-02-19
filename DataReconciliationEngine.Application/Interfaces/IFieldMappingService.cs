using DataReconciliationEngine.Application.DTOs;

namespace DataReconciliationEngine.Application.Interfaces;

/// <summary>
/// CRUD service for field mappings within a comparison configuration.
/// </summary>
public interface IFieldMappingService
{
    /// <summary>Returns all mappings for a given configuration.</summary>
    Task<List<FieldMappingDto>> GetByConfigIdAsync(int configId, CancellationToken ct = default);

    /// <summary>Returns a single mapping for editing.</summary>
    Task<FieldMappingDto?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Creates a new mapping under a configuration.</summary>
    Task<ServiceResult<FieldMappingDto>> CreateAsync(int configId, FieldMappingEditDto dto, CancellationToken ct = default);

    /// <summary>Updates an existing mapping.</summary>
    Task<ServiceResult<FieldMappingDto>> UpdateAsync(int id, FieldMappingEditDto dto, CancellationToken ct = default);

    /// <summary>Deletes a mapping.</summary>
    Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Toggles the IsActive flag on a mapping.</summary>
    Task<ServiceResult> ToggleActiveAsync(int id, bool isActive, CancellationToken ct = default);
}