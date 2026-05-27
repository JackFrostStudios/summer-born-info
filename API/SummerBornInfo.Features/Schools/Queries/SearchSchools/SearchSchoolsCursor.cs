using System.Text;
using System.Text.Json;

namespace SummerBornInfo.Features.Schools.Queries.SearchSchools;

public sealed record SearchSchoolsCursor(
    int NameMatchBucket,
    double TextRank,
    double NameSimilarity,
    double PostcodeSimilarity,
    double AddressSimilarity,
    string SearchNameNormalized,
    Guid SchoolId)
{
    private const int CurrentVersion = 1;

    public static string Encode(SearchSchoolsCursor cursor, string normalizedQuery)
    {
        var payload = new CursorPayload(
            CurrentVersion,
            normalizedQuery,
            cursor.NameMatchBucket,
            cursor.TextRank,
            cursor.NameSimilarity,
            cursor.PostcodeSimilarity,
            cursor.AddressSimilarity,
            cursor.SearchNameNormalized,
            cursor.SchoolId);
        var json = JsonSerializer.Serialize(payload);
        var bytes = Encoding.UTF8.GetBytes(json);

        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static bool TryDecode(string? encodedCursor, string normalizedQuery, out SearchSchoolsCursor cursor)
    {
        cursor = default!;

        if (string.IsNullOrWhiteSpace(encodedCursor))
        {
            return false;
        }

        byte[] bytes;
        try
        {
            var base64 = encodedCursor
                .Replace('-', '+')
                .Replace('_', '/');
            var padding = 4 - (base64.Length % 4);
            if (padding is > 0 and < 4)
            {
                base64 = base64.PadRight(base64.Length + padding, '=');
            }

            bytes = Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            return false;
        }

        CursorPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<CursorPayload>(bytes);
        }
        catch (JsonException)
        {
            return false;
        }

        if (payload is null
            || payload.Version != CurrentVersion
            || !string.Equals(payload.Query, normalizedQuery, StringComparison.Ordinal))
        {
            return false;
        }

        cursor = new SearchSchoolsCursor(
            payload.NameMatchBucket,
            payload.TextRank,
            payload.NameSimilarity,
            payload.PostcodeSimilarity,
            payload.AddressSimilarity,
            payload.SearchNameNormalized,
            payload.SchoolId);

        return true;
    }

    private sealed record CursorPayload(
        int Version,
        string Query,
        int NameMatchBucket,
        double TextRank,
        double NameSimilarity,
        double PostcodeSimilarity,
        double AddressSimilarity,
        string SearchNameNormalized,
        Guid SchoolId);
}
