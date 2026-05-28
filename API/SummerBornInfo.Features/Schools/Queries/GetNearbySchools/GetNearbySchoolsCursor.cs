namespace SummerBornInfo.Features.Schools.Queries.GetNearbySchools;

public sealed record GetNearbySchoolsCursor(
    double Latitude,
    double Longitude,
    double RadiusMiles,
    int? PageSize)
{
    private const int CurrentVersion = 1;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static bool TryDecode(string? encodedCursor, out GetNearbySchoolsCursor cursor)
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

        if (payload is null || payload.Version != CurrentVersion)
        {
            return false;
        }

        cursor = new GetNearbySchoolsCursor(
            payload.Latitude,
            payload.Longitude,
            payload.RadiusMiles,
            payload.PageSize);

        return true;
    }

    private sealed record CursorPayload(
        int Version,
        double Latitude,
        double Longitude,
        double RadiusMiles,
        int? PageSize);
}
