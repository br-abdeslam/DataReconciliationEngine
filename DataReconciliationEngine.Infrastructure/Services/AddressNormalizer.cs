using System.Text.RegularExpressions;

namespace DataReconciliationEngine.Infrastructure.Services;

/// <summary>
/// Deterministic text normalization for Belgian customer-site addresses.
/// All methods are pure functions — no side effects, no I/O.
/// </summary>
public static partial class AddressNormalizer
{
    // ── Compiled regexes (source-generated on .NET 8) ────────

    [GeneratedRegex(@"\s+")]
    private static partial Regex CollapseWhitespace();

    /// <summary>Matches "12 bus 3", "12 bte 3", "12 boîte 3", "12 box 3".</summary>
    [GeneratedRegex(@"^(\d+)\s*(?:BUS|BTE|BO[IÎ]TE|BOX)\s+(.+)$")]
    private static partial Regex BoxKeyword();

    /// <summary>Matches "12/3".</summary>
    [GeneratedRegex(@"^(\d+)\s*/\s*(.+)$")]
    private static partial Regex SlashSplit();

    /// <summary>Matches "12-3" only when right side is 1–2 digits (likely box, not range).</summary>
    [GeneratedRegex(@"^(\d+)\s*-\s*(\d{1,2})$")]
    private static partial Regex DashSplit();

    // ═════════════════════════════════════════════════════════
    //  Public API
    // ═════════════════════════════════════════════════════════

    /// <summary>
    /// Trim → collapse multiple spaces → UPPER.
    /// Returns null for null/empty/whitespace input.
    /// </summary>
    public static string? NormalizeText(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var collapsed = CollapseWhitespace().Replace(raw.Trim(), " ");
        return collapsed.ToUpperInvariant();
    }

    /// <summary>
    /// If <paramref name="boxRaw"/> is already filled, normalize both and return.
    /// Otherwise, try to extract a box from <paramref name="numberRaw"/> using
    /// Belgian patterns: "12 bus 3", "12/3", "12-3", "12 bte 3".
    /// </summary>
    public static (string? NumberNorm, string? BoxNorm) ParseNumberBox(
        string? numberRaw, string? boxRaw)
    {
        var normNumber = NormalizeText(numberRaw);
        var normBox = NormalizeText(boxRaw);

        // Box already present → keep both as-is (normalized)
        if (!string.IsNullOrEmpty(normBox))
            return (normNumber, normBox);

        // Nothing to parse
        if (string.IsNullOrEmpty(normNumber))
            return (null, null);

        // Try patterns in priority order
        var match = BoxKeyword().Match(normNumber);
        if (match.Success)
            return (match.Groups[1].Value.Trim(), NormalizeText(match.Groups[2].Value));

        match = SlashSplit().Match(normNumber);
        if (match.Success)
            return (match.Groups[1].Value.Trim(), NormalizeText(match.Groups[2].Value));

        match = DashSplit().Match(normNumber);
        if (match.Success)
            return (match.Groups[1].Value.Trim(), NormalizeText(match.Groups[2].Value));

        // No pattern matched — number stays, no box
        return (normNumber, null);
    }

    /// <summary>ROUND(value, 6) to match the SQL grouping precision.</summary>
    public static decimal? RoundCoordinate(decimal? value, int decimals = 6)
        => value.HasValue ? Math.Round(value.Value, decimals) : null;

    /// <summary>
    /// Quality score: higher = more complete / more trusted record.
    /// Used to pick the "master suggestion" per duplicate group.
    /// </summary>
    public static int ComputeCompletenessScore(
        string? street, string? zip, string? city,
        string? number, string? box,
        bool isVerified, bool fromKbo)
    {
        int score = 0;
        if (!string.IsNullOrWhiteSpace(street)) score += 20;
        if (!string.IsNullOrWhiteSpace(zip)) score += 20;
        if (!string.IsNullOrWhiteSpace(city)) score += 20;
        if (!string.IsNullOrWhiteSpace(number)) score += 15;
        if (!string.IsNullOrWhiteSpace(box)) score += 10;
        if (isVerified) score += 10;
        if (fromKbo) score += 5;
        return score; // max = 100
    }
}