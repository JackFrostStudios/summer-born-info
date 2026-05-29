namespace SummerBornInfo.Features.Schools.Queries.GetNearbySchools;

public static class GetNearbySchoolsRequestValidator
{
    public static bool TryValidate(GetNearbySchoolsRequest request, out GetNearbySchoolsRequest validatedRequest)
    {
        validatedRequest = default!;
        var effectivePageSize = request.PageSize ?? GetNearbySchoolsRequest.DefaultPageSize;

        if (!IsValidLatitude(request.Latitude)
            || !IsValidLongitude(request.Longitude)
            || !IsValidRadiusMiles(request.RadiusMiles)
            || request.PageSize is <= 0 or > GetNearbySchoolsRequest.MaximumPageSize)
        {
            return false;
        }

        var normalizedCursor = request.Cursor?.Trim();
        if (request.Cursor is not null)
        {
            if (string.IsNullOrWhiteSpace(normalizedCursor)
                || !GetNearbySchoolsCursor.TryDecode(normalizedCursor, out var cursor)
                || cursor.Latitude != request.Latitude
                || cursor.Longitude != request.Longitude
                || cursor.RadiusMiles != request.RadiusMiles
                || cursor.PageSize != effectivePageSize)
            {
                return false;
            }
        }

        validatedRequest = request with
        {
            Cursor = normalizedCursor,
        };

        return true;
    }

    private static bool IsValidLatitude(double? latitude)
    {
        return latitude.HasValue
            && !double.IsNaN(latitude.Value)
            && !double.IsInfinity(latitude.Value)
            && latitude.Value >= -90
            && latitude.Value <= 90;
    }

    private static bool IsValidLongitude(double? longitude)
    {
        return longitude.HasValue
            && !double.IsNaN(longitude.Value)
            && !double.IsInfinity(longitude.Value)
            && longitude.Value >= -180
            && longitude.Value <= 180;
    }

    private static bool IsValidRadiusMiles(double? radiusMiles)
    {
        return radiusMiles.HasValue
            && !double.IsNaN(radiusMiles.Value)
            && !double.IsInfinity(radiusMiles.Value)
            && radiusMiles.Value > 0
            && radiusMiles.Value <= GetNearbySchoolsRequest.MaximumRadiusMiles;
    }
}
