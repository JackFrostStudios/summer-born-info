namespace SummerBornInfo.Features.CsaApplicationReviews.Queries.GetAdminCsaApplicationReviewQueue;

public sealed record AdminCsaApplicationReviewQueueCursor(
    string QueueStates,
    int PageSize,
    DateTimeOffset LatestReportAtUtc,
    Guid ReviewId)
{
    private const int CurrentVersion = 1;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static string Encode(AdminCsaApplicationReviewQueueCursor cursor)
    {
        var payload = new CursorPayload(
            CurrentVersion,
            cursor.QueueStates,
            cursor.PageSize,
            cursor.LatestReportAtUtc,
            cursor.ReviewId);
        var json = JsonSerializer.Serialize(payload);
        var bytes = Encoding.UTF8.GetBytes(json);

        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static bool TryDecode(string? encodedCursor, string expectedQueueStates, out AdminCsaApplicationReviewQueueCursor cursor)
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
            || payload.ReviewId == Guid.Empty
            || !string.Equals(payload.QueueStates, expectedQueueStates, StringComparison.Ordinal))
        {
            return false;
        }

        cursor = new AdminCsaApplicationReviewQueueCursor(
            payload.QueueStates,
            payload.PageSize,
            payload.LatestReportAtUtc,
            payload.ReviewId);
        return true;
    }

    private sealed record CursorPayload(
        int Version,
        string QueueStates,
        int PageSize,
        DateTimeOffset LatestReportAtUtc,
        Guid ReviewId);
}
