namespace DataReconciliationEngine.Application.DTOs;

/// <summary>DTO for the duplicate groups table (server-side paged).</summary>
public sealed class DuplicateGroupDto
{
    public required int Id { get; init; }
    public Guid? GroupId { get; init; }
    public decimal? LatRound { get; init; }
    public decimal? LonRound { get; init; }
    public required string CandidateKey { get; init; }
    public required int RecordsCount { get; init; }
    public required DateTime CreatedAt { get; init; }
}