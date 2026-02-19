namespace DataReconciliationEngine.Application.DTOs;

/// <summary>
/// Summary returned after a comparison run completes.
/// Maps 1-to-1 with the persisted <see cref="Domain.Entities.ComparisonRun"/> totals.
/// </summary>
public sealed class RunComparisonSummary
{
    /// <summary>ID of the persisted <see cref="Domain.Entities.ComparisonRun"/>.</summary>
    public required int RunId { get; init; }

    /// <summary>ID of the configuration that was executed.</summary>
    public required int ComparisonConfigId { get; init; }

    /// <summary>Total matched key pairs evaluated.</summary>
    public required int TotalRecords { get; init; }

    /// <summary>Matched keys with at least one field difference.</summary>
    public required int TotalMismatches { get; init; }

    /// <summary>Keys present in System A but missing from System B.</summary>
    public required int TotalMissingInB { get; init; }

    /// <summary>Keys present in System B but missing from System A.</summary>
    public required int TotalMissingInA { get; init; }
}