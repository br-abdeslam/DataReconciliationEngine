namespace DataReconciliationEngine.Application.DTOs;

/// <summary>DTO for the duplicate record detail drawer.</summary>
public sealed class DuplicateRecordDto
{
    public required int Id { get; init; }
    public required int CustomerSitesId { get; init; }

    public string? StreetRaw { get; init; }
    public string? NumberRaw { get; init; }
    public string? BoxRaw { get; init; }
    public string? ZipRaw { get; init; }
    public string? CityRaw { get; init; }

    public string? StreetNorm { get; init; }
    public string? NumberNorm { get; init; }
    public string? BoxNorm { get; init; }
    public string? ZipNorm { get; init; }
    public string? CityNorm { get; init; }

    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }

    public required int CompletenessScore { get; init; }
    public required bool IsMasterSuggested { get; init; }
    public string? Reason { get; init; }
}