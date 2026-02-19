using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataReconciliationEngine.Application.DTOs;

/// <summary>
/// Request DTO that tells the comparison engine which configuration to execute.
/// </summary>
public sealed class RunComparisonRequest
{
    /// <summary>
    /// The ID of the <see cref="Domain.Entities.TableComparisonConfiguration"/> to run.
    /// </summary>
    public required int ComparisonConfigId { get; init; }

    // Placeholder for Sprint 2+ – optional filters (e.g. WHERE clause, date range).
    // public string? FilterSystemA { get; init; }
    // public string? FilterSystemB { get; init; }
}