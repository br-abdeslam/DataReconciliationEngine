using DataReconciliationEngine.Application.DTOs;

namespace DataReconciliationEngine.Application.Interfaces;

/// <summary>
/// Detects duplicate rows in Company.dbo.customer_sites
/// grouped by (Group_ID, rounded Latitude, rounded Longitude).
/// Results are stored in LocalDB; source DB is read-only.
/// </summary>
public interface ICustomerSitesDuplicateDetector
{
    Task<DuplicateDetectionSummary> RunDetectionAsync(CancellationToken ct = default);
}