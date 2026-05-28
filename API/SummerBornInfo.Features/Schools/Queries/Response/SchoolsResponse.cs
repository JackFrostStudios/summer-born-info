namespace SummerBornInfo.Features.Schools.Queries.Response;

public sealed record SchoolsResponse
{
    public required IReadOnlyList<SchoolResponse> Schools { get; init; }
    public required string? NextCursor { get; init; }

    public static SchoolsResponse Create(IReadOnlyList<SchoolResponse> schools, Guid? nextCursor)
    {
        return new SchoolsResponse
        {
            Schools = schools,
            NextCursor = nextCursor?.ToString(),
        };
    }
}
