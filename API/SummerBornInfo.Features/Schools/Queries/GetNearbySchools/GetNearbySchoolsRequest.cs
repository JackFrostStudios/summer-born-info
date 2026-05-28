namespace SummerBornInfo.Features.Schools.Queries.GetNearbySchools;

public sealed record GetNearbySchoolsRequest(
    double? Latitude,
    double? Longitude,
    double? RadiusMiles,
    string? Cursor = null,
    int? PageSize = null)
{
    public const int DefaultPageSize = 100;
    public const int MaximumPageSize = 200;
    public const double MaximumRadiusMiles = 100;
}
