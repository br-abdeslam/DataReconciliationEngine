using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataReconciliationEngine.Application.DTOs;

namespace DataReconciliationEngine.Application.Interfaces;

/// <summary>
/// Core use-case contract: runs a full comparison between System A and System B
/// based on a stored <see cref="Domain.Entities.TableComparisonConfiguration"/>.
/// </summary>
public interface IComparisonEngine
{
    /// <summary>
    /// Executes the comparison and persists results (Missing + Mismatches only).
    /// </summary>
    /// <param name="request">Identifies which comparison configuration to run.</param>
    /// <param name="cancellationToken">Allows the caller to cancel long-running comparisons.</param>
    /// <returns>A summary of the completed run.</returns>
    Task<RunComparisonSummary> RunComparisonAsync(
        RunComparisonRequest request,
        CancellationToken cancellationToken = default);
}