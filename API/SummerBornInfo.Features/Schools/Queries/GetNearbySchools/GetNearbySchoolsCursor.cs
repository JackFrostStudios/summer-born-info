namespace SummerBornInfo.Features.Schools.Queries.GetNearbySchools;

public sealed record GetNearbySchoolsCursor(
    double Latitude,
    double Longitude,
    double RadiusMiles,
    int PageSize,
    double DistanceMeters,
    Guid SchoolId)
{
    private const int CurrentVersion = 1;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static string Encode(GetNearbySchoolsCursor cursor)
    {
        var payload = new CursorPayload(
            CurrentVersion,
            cursor.Latitude,
            cursor.Longitude,
            cursor.RadiusMiles,
            cursor.PageSize,
            cursor.DistanceMeters,
            cursor.SchoolId);
        var json = JsonSerializer.Serialize(payload);
        var bytes = Encoding.UTF8.GetBytes(json);

        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

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

        if (payload.PageSize <= 0
            || double.IsNaN(payload.DistanceMeters)
            || double.IsInfinity(payload.DistanceMeters)
            || payload.DistanceMeters < 0
            || payload.SchoolId == Guid.Empty)
        {
            return false;
        }

        cursor = new GetNearbySchoolsCursor(
            payload.Latitude,
            payload.Longitude,
            payload.RadiusMiles,
            payload.PageSize,
            payload.DistanceMeters,
            payload.SchoolId);

        return true;
    }

    private sealed record CursorPayload(
        int Version,
        double Latitude,
        double Longitude,
        double RadiusMiles,
        int PageSize,
        double DistanceMeters,
        Guid SchoolId);
}
