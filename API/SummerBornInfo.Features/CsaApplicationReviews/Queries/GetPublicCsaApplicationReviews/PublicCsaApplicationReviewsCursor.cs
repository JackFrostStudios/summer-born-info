namespace SummerBornInfo.Features.CsaApplicationReviews.Queries.GetPublicCsaApplicationReviews;

public sealed record PublicCsaApplicationReviewsCursor(
    int PageSize,
    DateTimeOffset SubmittedAtUtc,
    Guid ReviewId)
{
    private const int CurrentVersion = 1;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static string Encode(PublicCsaApplicationReviewsCursor cursor)
    {
        var payload = new CursorPayload(
            CurrentVersion,
            cursor.PageSize,
            cursor.SubmittedAtUtc,
            cursor.ReviewId);
        var json = JsonSerializer.Serialize(payload);
        var bytes = Encoding.UTF8.GetBytes(json);

        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static bool TryDecode(string? encodedCursor, out PublicCsaApplicationReviewsCursor cursor)
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
            payload = JsonSerializer.Deserialize<CursorPayload>(bytes, SerializerOptions);
        }
        catch (JsonException)
        {
            return false;
        }

        if (payload is null
            || payload.Version != CurrentVersion
            || payload.PageSize <= 0
            || payload.ReviewId == Guid.Empty)
        {
            return false;
        }

        cursor = new PublicCsaApplicationReviewsCursor(
            payload.PageSize,
            payload.SubmittedAtUtc,
            payload.ReviewId);
        return true;
    }

    private sealed record CursorPayload(
        int Version,
        int PageSize,
        DateTimeOffset SubmittedAtUtc,
        Guid ReviewId);
}
